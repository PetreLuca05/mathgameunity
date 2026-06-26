using Unity.VisualScripting;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public enum PhysicsState { Kinematic, Ragdoll }
    public enum OwnerType { Player, NPC }

    [Header("State Machine")]
    public PhysicsState physicsState = PhysicsState.Kinematic;
    public OwnerType ownerType = OwnerType.Player;

    [Header("Blinking")]
    [Tooltip("Minimum seconds between blinks")]
    public float blinkIntervalMin = 2.0f;
    [Tooltip("Maximum seconds between blinks")]
    public float blinkIntervalMax = 4.0f;
    [Tooltip("Assign the eyes transform for blinking animation")]
    public Transform eyesTransform;
    [Tooltip("How long the blink stays closed")] public float blinkClosedTime = 0.08f;
    [Tooltip("How long the blink takes to close/open")] public float blinkTransitionTime = 0.06f;  

    [Header("Squish Settings")]
    [Tooltip("Minimum squish scale (more = less squish)")]
    public float squishAmountMin = 0.6f;
    [Tooltip("Maximum squish scale (less = more squish)")]
    public float squishAmountMax = 1.0f;
    [Tooltip("Time to stay squished")] public float squishTime = 0.08f;
    [Tooltip("Time to recover to normal")] public float recoverTime = 0.18f;

    [Header("Dialogue Settings")]
    public Transform dialogPanelTransform;

    public Animator animator;
    Transform playerModel;
    float fallStartY;
    bool wasGroundedLastFrame;

    PlayerMovement playerMovement;
    Rigidbody rb;
    bool grounded;
    Collider masterCollider;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerModel = transform;
        masterCollider = GetComponentInParent<Collider>();
        // Start randomized blinking
        StartCoroutine(BlinkLoop());
        SetPhysicsState(physicsState);
    }

    private void Update()
    {   
        if(!rb) return;

        float velocity = rb.linearVelocity.magnitude;
        animator.SetFloat("Velocity", velocity);

        grounded = playerMovement.grounded;

        // Track fall start and landing
        if (!wasGroundedLastFrame && grounded)
        {
            float fallHeight = fallStartY - transform.position.y;
            if (fallHeight > 0.5f)
                StartCoroutine(SquishOnLand(Mathf.Clamp(fallHeight, 0.5f, 8f)));
        }
        if (!grounded && wasGroundedLastFrame)
        {
            fallStartY = transform.position.y;
        }
        wasGroundedLastFrame = grounded;
    }

    // State machine logic
    public void SetPhysicsState(PhysicsState state)
    {
        physicsState = state;
        if (state == PhysicsState.Kinematic)
        {
            SetKinematic(true);
        }
        else if (state == PhysicsState.Ragdoll)
        {
            SetKinematic(false);
        }
    }

    private void SetKinematic(bool isKinematic)
    {
        var rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (var r in rigidbodies)
        {
            if (isKinematic)
            {
                r.isKinematic = isKinematic;
            }
            else
            {
                if (!r.gameObject.CompareTag("RAGDOLL_ROOT"))
                {
                    r.isKinematic = isKinematic;
                } 
                //else Debug.Log("Skipping RAGDOLL_ROOT Rigidbody");
            }
        }

        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
        {
            c.enabled = !isKinematic;
        }

        if (animator != null)
            animator.enabled = isKinematic;

        if (masterCollider != null)
            masterCollider.enabled = isKinematic;
            
    }

    // Coroutine for random blinking
    private System.Collections.IEnumerator BlinkLoop()
    {
        yield return new WaitForSeconds(Random.Range(blinkIntervalMin, blinkIntervalMax));
        while (true)
        {
            TriggerBlink();
            yield return new WaitForSeconds(Random.Range(blinkIntervalMin, blinkIntervalMax));
        }
    }
    

    // Call this to trigger a blink
    public void TriggerBlink()
    {
        if (eyesTransform != null)
            StartCoroutine(BlinkCoroutine());
    }

    private System.Collections.IEnumerator BlinkCoroutine()
    {
        Vector3 originalScale = eyesTransform.localScale;
        Vector3 closedScale = new Vector3(originalScale.x, originalScale.y, 0.01f);
        // Close eyes
        float t = 0;
        while (t < blinkTransitionTime)
        {
            eyesTransform.localScale = Vector3.Lerp(originalScale, closedScale, t / blinkTransitionTime);
            t += Time.deltaTime;
            yield return null;
        }
        eyesTransform.localScale = closedScale;
        yield return new WaitForSeconds(blinkClosedTime);
        // Open eyes
        t = 0;
        while (t < blinkTransitionTime)
        {
            eyesTransform.localScale = Vector3.Lerp(closedScale, originalScale, t / blinkTransitionTime);
            t += Time.deltaTime;
            yield return null;
        }
        eyesTransform.localScale = originalScale;
    }

    // Squish effect coroutine
    private System.Collections.IEnumerator SquishOnLand(float fallHeight)
    {
        Vector3 originalScale = playerModel.localScale;
        float squishAmount = Mathf.Lerp(squishAmountMax, squishAmountMin, fallHeight / 8f); // More fall = more squish
        // Squish down
        playerModel.localScale = new Vector3(originalScale.x * (2.0f - squishAmount), originalScale.y * squishAmount, originalScale.z * (2.0f - squishAmount));
        yield return new WaitForSeconds(squishTime);
        // Recover
        float t = 0;
        while (t < recoverTime)
        {
            playerModel.localScale = Vector3.Lerp(playerModel.localScale, originalScale, t / recoverTime);
            t += Time.deltaTime;
            yield return null;
        }
        playerModel.localScale = originalScale;
    }
}
