using UnityEngine;

[RequireComponent(typeof(Collider))]

public class InteractParent : MonoBehaviour
{
    [Header("Interact Settings")]
    public bool isInteractable = true;
    public bool isInteractingWithMe = false;
    public float timeToInteract = 0f;
    public string interactPrompt = "Interact";
    Collider myCollider;
    public float interactDistance = 3f;

    // Timer logic for holding interact
    private float interactHoldTimer = 0f;
    private bool isHoldingInteract = false;
    private bool hasInteracted = false;

    void Start()
    {
        myCollider = GetComponent<Collider>();
    }

    public virtual void StartInteractTimer()
    {
        if(isInteractable == false)
            return;

        //Debug.Log("Starting interaction with " + gameObject.name);
        isInteractingWithMe = true;
        interactHoldTimer = 0f;
        isHoldingInteract = true;
        hasInteracted = false;
    }

    public virtual void StopInteractTimer()
    {
        //Debug.Log("Stopping interaction with " + gameObject.name);
        isInteractingWithMe = false;
        interactHoldTimer = 0f;
        isHoldingInteract = false;
        hasInteracted = false;
    }

    public virtual void Interact()
    {
        //Debug.Log("Interacted with " + gameObject.name);
        isInteractable = false;
    }

    public virtual void QuitInteract()
    {
        //Debug.Log("Quit with" + gameObject.name);
        isInteractingWithMe = false;
        isInteractable = true;
    }

    void FixedUpdate()
    {
        UpdateInteractHold(Time.fixedDeltaTime);
        if (myCollider is SphereCollider sphere) sphere.radius = interactDistance;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, interactDistance);
    }

    void OnValidate()
    {
        if (myCollider == null)
            myCollider = GetComponent<Collider>();
        if (myCollider is SphereCollider sphere)
            sphere.radius = interactDistance;
    }

    public void UpdateInteractHold(float deltaTime)
    {
        if (!isHoldingInteract || hasInteracted || !isInteractable)
            return;

        interactHoldTimer += deltaTime;
        if (interactHoldTimer >= timeToInteract)
        {
            Interact();
            hasInteracted = true;
        }
    }
}
