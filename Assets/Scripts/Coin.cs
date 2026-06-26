using UnityEngine;

public class Coin : MonoBehaviour
{
    public GameObject model;
    float moveSpeed = 1.5f;
    float archHeight = 2f;
    Transform targetTransform;
    Vector3 startPos;
    float journeyTime;

    Vector3 spinAxis;
    float spinSpeed;
    Quaternion initialRotation;
    bool collected = false;

    public enum CoinTargetState { None, Player, Ground }
    private CoinTargetState _targetState = CoinTargetState.None;
    public CoinTargetState targetState
    {
        get => _targetState;
        set
        {
            if (_targetState != value)
            {
                var oldState = _targetState;
                _targetState = value;
                OnTargetStateChanged(oldState, _targetState);
            }
        }
    }

    private void OnTargetStateChanged(CoinTargetState oldState, CoinTargetState newState)
    {
        if (newState == CoinTargetState.None)
        {
            GetComponent<Collider>().enabled = true;
            Debug.Log("Coin dropped to ground.");
        }
        else
        {
            GetComponent<Collider>().enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (targetState == CoinTargetState.None && other.CompareTag("Player") && other.GetComponentInChildren<PlayerLogic_Quest_CoinCollect>() != null)
        {
            GoToPlayer(other.transform);
        }
    }

    public void GoToPlayer(Transform playerTransform)
    {
        GoToTransform(moveSpeed, archHeight, playerTransform, CoinTargetState.Player);
    }

    public void GoToGround(Transform groundTarget)
    {
        GoToTransform(moveSpeed, archHeight, groundTarget, CoinTargetState.Ground);
    }

    public void GoToTransform(float _moveSpeed, float _archHeight, Transform _targetTransform, CoinTargetState state)
    {
        moveSpeed = _moveSpeed;
        archHeight = _archHeight;
        if (_targetTransform != null)
        {
            targetTransform = _targetTransform;
            startPos = transform.position;
            journeyTime = 0f;
            targetState = state;
            collected = false;
        }
        spinAxis = Random.onUnitSphere;
        spinSpeed = Random.Range(360f, 1080f);
        initialRotation = model.transform.rotation;
    }

    void Update()
    {
        if (targetState == CoinTargetState.None || targetTransform == null)
            return;

        journeyTime += Time.deltaTime * moveSpeed;
        Vector3 endPos = targetTransform.position;
        float t = Mathf.Clamp01(journeyTime);
        // Calculate arch position
        Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
        float arch = Mathf.Sin(Mathf.PI * t) * archHeight;
        currentPos.y += arch;
        transform.position = currentPos;

        float spinAngle = spinSpeed * journeyTime;
        model.transform.rotation = initialRotation * Quaternion.AngleAxis(spinAngle, spinAxis);

        if (targetState == CoinTargetState.Player && !collected && t >= 0.5f)
        {
            // Notify player only once, after halfway to player
            targetTransform.GetComponentInChildren<PlayerLogic_Quest_CoinCollect>()?.OnCoinCollected();
            collected = true;
        }

        if (t >= 1f)
        {
            if (targetState == CoinTargetState.Player)
            {
                // Reached player, destroy coin
                Destroy(gameObject);
                return;
            }
            else if (targetState == CoinTargetState.Ground)
            {
                model.transform.rotation = targetTransform.rotation;
                // Reached ground, stop moving
                targetState = CoinTargetState.None;
            }
        }
    }
}
