using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class Catapult : InteractParent
{

    [System.Serializable]
    public struct LaunchPoint
    {
        public Transform launchDestination;
        public float duration;
        public float arcHeight;
        public GameObject destinationCamera;

        [Header("Bounce Settings")]
        public float numberOfBounces;
        public float bounceStartHeight;
        public float bounceDuration;
        public float bounceMaxDistance;
    }

    [Header("Catapult Settings")]
    public LaunchPoint[] launchPoints;
    public Transform catapultPivot;
    public float catapultRotateSpeed = 5f;
    public Transform playerHoldPoint;

    [Header("Camera Settings")]
    public GameObject catapultCamera;
    public GameObject launchCamera;
    public GameObject landingCamera;

    [Header("Input Settings")]
    // Input System reference for custom "Use" key
    public InputActionReference useActionReference;
    public InputActionReference moveAction;
    float loadAmount;
    Animator catapultAnimator;

    public Vector2 rotationClamp = new Vector2(-60f, 60f); // Min and Max Y rotation

    void Start()
    {
        catapultAnimator = GetComponentInChildren<Animator>();
        catapultCamera.SetActive(false);
        launchCamera.SetActive(false);
        landingCamera.SetActive(false);

        var allCameras = GetComponentsInChildren<CinemachineVirtualCamera>();
        foreach (var cam in allCameras)
        {
            cam.Follow = GameManager.instance.GetComponentInPlayer<Transform>();
        }
    }

    void Update()
    {
        loadAmount = Mathf.Lerp(loadAmount, isInteractingWithMe ? 1f : 0f, Time.deltaTime * 5f);
        catapultAnimator.SetFloat("LoadAmount", loadAmount);

        if (useActionReference.action.WasPerformedThisFrame() && isInteractingWithMe)
        {
            StartCoroutine(LaunchPlayerArc());
        }

        if (isInteractingWithMe)
        {
            float rotateInput = 0f;
            if (moveAction != null)
            {
                Vector2 move = -moveAction.action.ReadValue<Vector2>();
                rotateInput = move.x;
            }

            if (Mathf.Abs(rotateInput) > 0.01f)
            {
                catapultPivot.Rotate(Vector3.up, rotateInput * catapultRotateSpeed * 20f * Time.deltaTime, Space.World);

                // Clamp rotation after rotating
                Vector3 euler = catapultPivot.localEulerAngles;
                // Convert to -180..180 range
                float y = euler.y > 180f ? euler.y - 360f : euler.y;
                y = Mathf.Clamp(y, rotationClamp.x, rotationClamp.y);
                catapultPivot.localEulerAngles = new Vector3(euler.x, y, euler.z);
            }

            Transform playerTransform = GameManager.instance.GetComponentInPlayer<Transform>();
            playerTransform.position = playerHoldPoint.position;
            playerTransform.rotation = playerHoldPoint.rotation;
        }
    }

    public override void Interact()
    {
        base.Interact();
        GameManager.instance.GetComponentInPlayer<PlayerManager>().SetBehaviourState(BehaviourState.inCatapult);
        GameManager.instance.GetComponentInPlayer<InteractManager>().ClearFloundInteractable();

        catapultCamera.SetActive(true);
        launchCamera.SetActive(false);
        landingCamera.SetActive(false);
    }

    public override void QuitInteract()
    {
        base.QuitInteract();
        GameManager.instance.GetComponentInPlayer<PlayerManager>().SetBehaviourState(BehaviourState.Default);
        catapultCamera.SetActive(false);
        launchCamera.SetActive(false);
    }

    public void onLand(Transform landTransform = null, Transform playerModel = null)
    {
        base.QuitInteract();
        GameManager.instance.GetComponentInPlayer<PlayerManager>().SetBehaviourState(BehaviourState.inLandAfterLaunch, landingCamera);
        catapultCamera.SetActive(false);
        launchCamera.SetActive(false);
        if (playerModel != null && landTransform != null){
            Vector3 euler = landTransform.rotation.eulerAngles;
            euler.y += 90f;
            playerModel.rotation = Quaternion.Euler(euler);
        }
    }

    IEnumerator LaunchPlayerArc()
    {
        catapultCamera.SetActive(false);
        launchCamera.transform.position = catapultCamera.transform.position;
        launchCamera.SetActive(true);

        GameManager.instance.GetComponentInPlayer<PlayerManager>().SetBehaviourState(BehaviourState.inLaunch);

        Transform playerTransform = GameManager.instance.GetComponentInPlayer<PlayerMovement>().transform;
        Transform playerModel = GameManager.instance.GetComponentInPlayer<PlayerMovement>().playerModel;

        LaunchPoint launch = launchPoints.Length > 0 ? launchPoints[0] : default;
        Vector3 start = playerHoldPoint.position;
        Vector3 end = launch.launchDestination != null ? launch.launchDestination.position : start;
        float duration = launch.duration > 0 ? launch.duration : 1f;
        float arcHeight = launch.arcHeight > 0 ? launch.arcHeight : 5f;

        float elapsed = 0f;
        bool cameraSnapped = false;

        Vector3 spinAxis = Random.onUnitSphere;
        float spinSpeed = Random.Range(360f, 1080f);
        Quaternion initialRotation = playerModel.rotation;

        // Arc flight
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 currentPos = Vector3.Lerp(start, end, t);
            currentPos.y += arcHeight * 4 * t * (1 - t);

            playerTransform.position = currentPos;

            // Snap camera to destination at halfway point
            if (!cameraSnapped && t >= 0.5f)
            {
                landingCamera.SetActive(true);
                launchCamera.SetActive(false);
                if (launch.destinationCamera != null)
                    launch.destinationCamera.SetActive(true);
                cameraSnapped = true;
            }

            // Spin simulation
            float spinAngle = spinSpeed * elapsed;
            playerModel.rotation = initialRotation * Quaternion.AngleAxis(spinAngle, spinAxis);

            elapsed += Time.deltaTime;
            yield return null;
        }
        // --- Simulate bounces using numberOfBounces, bounceStartHeight, bounceDuration, bounceMaxDistance ---
        int bounces = Mathf.Max(1, Mathf.RoundToInt(launch.numberOfBounces));
        float bounceHeight = launch.bounceStartHeight > 0 ? launch.bounceStartHeight : arcHeight * 0.5f;
        float bounceDuration = launch.bounceDuration > 0 ? launch.bounceDuration : 0.3f;
        float bounceDamp = 0.5f; // Each bounce is half as high and half as long as the previous
        Vector3 landedPos = end;
        Vector3 forwardDir = (end - start).normalized;

        onLand(launch.launchDestination, playerModel);

        // Calculate initial total forward bounce distance, clamped to bounceMaxDistance if set
        float totalForwardDistance = Vector3.Distance(start, end) * 0.15f;
        if (launch.bounceMaxDistance > 0)
            totalForwardDistance = Mathf.Min(totalForwardDistance, launch.bounceMaxDistance);

        // Distribute forward distance over bounces (geometric series sum)
        float sum = 0f;
        float temp = 1f;
        for (int i = 0; i < bounces; i++)
        {
            sum += temp;
            temp *= bounceDamp;
        }
        float initialBounceForwardDistance = sum > 0 ? totalForwardDistance / sum : 0f;

        float currentBounceForwardDistance = initialBounceForwardDistance;

        for (int i = 0; i < bounces; i++)
        {
            float t = 0f;
            Vector3 bounceStart = landedPos;
            Vector3 bounceEnd = bounceStart + forwardDir * currentBounceForwardDistance;
            float thisBounceHeight = bounceHeight;
            float thisBounceDuration = bounceDuration;

            while (t < 1f)
            {
                float parabola = 4 * thisBounceHeight * t * (1 - t); // Parabolic bounce
                Vector3 bouncePos = Vector3.Lerp(bounceStart, bounceEnd, t);
                bouncePos.y += parabola;
                playerTransform.position = bouncePos;
                t += Time.deltaTime / thisBounceDuration;
                yield return null;
            }

            landedPos = bounceEnd;
            bounceHeight *= bounceDamp;
            bounceDuration *= bounceDamp;
            currentBounceForwardDistance *= bounceDamp;
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (launchPoints != null && launchPoints.Length > 0 && playerHoldPoint != null)
        {
            foreach (var launch in launchPoints)
            {
                if (launch.launchDestination == null) continue;
                Vector3 start = playerHoldPoint.position;
                Vector3 end = launch.launchDestination.position;
                int segments = 20;
                Vector3 prevPoint = start;

                // Draw arc (yellow)
                for (int i = 1; i <= segments; i++)
                {
                    float t = i / (float)segments;
                    Vector3 point = Vector3.Lerp(start, end, t);
                    point.y += launch.arcHeight * 4 * t * (1 - t);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(prevPoint, point);
                    prevPoint = point;
                }

                // Draw bounces (purple)
                int bounces = Mathf.Max(1, Mathf.RoundToInt(launch.numberOfBounces));
                float bounceHeight = launch.bounceStartHeight > 0 ? launch.bounceStartHeight : launch.arcHeight * 0.5f;
                float bounceDamp = 0.5f;
                float bounceDuration = launch.bounceDuration > 0 ? launch.bounceDuration : 0.3f;
                Vector3 landedPos = end;
                Vector3 forwardDir = (end - start).normalized;

                float totalForwardDistance = Vector3.Distance(start, end) * 0.15f;
                if (launch.bounceMaxDistance > 0)
                    totalForwardDistance = Mathf.Min(totalForwardDistance, launch.bounceMaxDistance);

                float sum = 0f;
                float temp = 1f;
                for (int i = 0; i < bounces; i++)
                {
                    sum += temp;
                    temp *= bounceDamp;
                }
                float initialBounceForwardDistance = sum > 0 ? totalForwardDistance / sum : 0f;
                float currentBounceForwardDistance = initialBounceForwardDistance;

                for (int i = 0; i < bounces; i++)
                {
                    Vector3 bounceStart = landedPos;
                    Vector3 bounceEnd = bounceStart + forwardDir * currentBounceForwardDistance;
                    Vector3 prevBounce = bounceStart;
                    int bounceSegments = 12;
                    for (int j = 1; j <= bounceSegments; j++)
                    {
                        float t = j / (float)bounceSegments;
                        float parabola = 4 * bounceHeight * t * (1 - t);
                        Vector3 bouncePoint = Vector3.Lerp(bounceStart, bounceEnd, t);
                        bouncePoint.y += parabola;
                        Gizmos.color = new Color(0.6f, 0f, 1f, 1f); // Purple
                        Gizmos.DrawLine(prevBounce, bouncePoint);
                        prevBounce = bouncePoint;
                    }
                    landedPos = bounceEnd;
                    bounceHeight *= bounceDamp;
                    bounceDuration *= bounceDamp;
                    currentBounceForwardDistance *= bounceDamp;
                }
            }
        }
    }
}
