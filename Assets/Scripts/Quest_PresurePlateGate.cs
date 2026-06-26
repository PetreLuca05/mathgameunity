using Unity.Cinemachine;
using UnityEngine;

public class Quest_PresurePlateGate : QuestParent
{
    [Header("Gate Settings")]
    public PreasurePlate[] preasurePlatesInOrderToActivate;
    [HideInInspector] public PreasurePlate[] activatedPreasurePlates;
    public Transform gateModel;
    public float closedPositionY = 0f;
    public float openPositionY = 3f;

    public float openDuration = 2f;
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float shakeTriggerDelay = 1f;

    Vector3 closedPosition;
    Vector3 openPosition;
    float openTimer = 0f;
    bool opening = false;
    bool impulseTriggered = false;
    CinemachineImpulseSource impulseSource;

    void Start()
    {
        activatedPreasurePlates = new PreasurePlate[preasurePlatesInOrderToActivate.Length];
        impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        if (gateModel != null)
        {
            closedPosition = gateModel.localPosition;
            closedPosition.y = closedPositionY;
            openPosition = closedPosition;
            openPosition.y = openPositionY;
            gateModel.localPosition = closedPosition;
        }
    }

    void Update()
    {
        if (opening && gateModel != null)
        {
            openTimer += Time.deltaTime;
            float t = Mathf.Clamp01(openTimer / openDuration);
            float curveT = openCurve.Evaluate(t);
            gateModel.localPosition = Vector3.Lerp(closedPosition, openPosition, curveT);

            if (curveT >= shakeTriggerDelay && impulseSource != null && !impulseTriggered)
            {
                impulseSource.GenerateImpulse();
                impulseTriggered = true;
            }

            if (t >= 1f){
                opening = false;
                Destroy(this);
            }
        }
    }

    public int AddPreasurePlateToActivatedList(PreasurePlate plate)
    {
        for (int i = 0; i < activatedPreasurePlates.Length; i++)
        {
            if (activatedPreasurePlates[i] == plate)
                return i; // Already in the list, return its position
        }

        for (int i = 0; i < activatedPreasurePlates.Length; i++)
        {
            if (activatedPreasurePlates[i] == null)
            {
                activatedPreasurePlates[i] = plate;
                if(opening == false)
                    StartCoroutine(DelayedCheckAllPlatesActivated());
                return i; // Return the position it was added to
            }
        }

        return -1; // Not added (list full)
    }

    public void RemovePreasurePlateFromActivatedList(int index)
    {
        if (index >= 0 && index < activatedPreasurePlates.Length)
        {
            activatedPreasurePlates[index] = null;
        }
    }

    private System.Collections.IEnumerator DelayedCheckAllPlatesActivated()
    {
        yield return new WaitForSeconds(0.3f); // Adjust delay as needed
        CheckAllPlatesActivated();
    }

    void CheckAllPlatesActivated()
    {
        // Only check if all plates are pressed
        bool allPressed = true;
        for (int i = 0; i < activatedPreasurePlates.Length; i++)
        {
            if (activatedPreasurePlates[i] == null)
            {
                allPressed = false;
                break;
            }
        }

        if (!allPressed)
            return;

        // Now check sequence
        bool allCorrect = true;
        for (int i = 0; i < preasurePlatesInOrderToActivate.Length; i++)
        {
            if (activatedPreasurePlates[i] != preasurePlatesInOrderToActivate[i])
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            // All plates are activated in the correct order
            opening = true;
            openTimer = 0f;
            impulseTriggered = false;
            for (int i = 0; i < activatedPreasurePlates.Length; i++)
            {
                if (activatedPreasurePlates[i] != null)
                {
                    activatedPreasurePlates[i].SetState(PlateState.Correct);
                    base.CompleteQuest();
                }
            }
        }
        else
        {
            // Incorrect sequence, mark plates as incorrect
            for (int i = 0; i < activatedPreasurePlates.Length; i++)
            {
                if (activatedPreasurePlates[i] != null)
                {
                    activatedPreasurePlates[i].SetState(PlateState.Incorrect);
                }
            }
        }
    }
}

