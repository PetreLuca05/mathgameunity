using UnityEngine;
using UnityEngine.UI;

public enum PlateState { NotPressed, Pressed, Correct, Incorrect }

public class PreasurePlate : MonoBehaviour
{   
    private PlateState currentState = PlateState.NotPressed;

    public float pressDepth = 0.1f;
    public float resetDelay = 2f;
    public Material baseMaterial;
    public Texture buttonTexture;
    public Color notPressedColor = Color.white;
    public Color pressedColor = Color.gray;
    public Color correctSequenceColor = Color.green;
    public Color incorrectSequenceColorPrimary = Color.red;
    public Color incorrectSequenceColorSecondary = Color.yellow;
    public float pressSpeed = 10f;
    public float incorrectPulseSpeed = 5f;

    Transform model;
    Collider plateCollider;
    float activeTimer = 0;
    Material materialInstance;
    float incorrectPulseTimer = 0f;
    Quest_PresurePlateGate linkedGate;

    [HideInInspector] public int listPositionInGate = -1;
    [HideInInspector] public bool correctSequence = false;

    void Start()
    {
        model = transform.GetChild(0);
        linkedGate = GetComponentInParent<Quest_PresurePlateGate>();
        plateCollider = GetComponent<Collider>();
        if (baseMaterial != null)
        {
            materialInstance = new Material(baseMaterial);
            materialInstance.color = notPressedColor;
            if (buttonTexture != null)
                materialInstance.mainTexture = buttonTexture;
        }
        if (materialInstance != null)
            model.GetComponentInChildren<Renderer>().material = materialInstance;
    }

    public void SetState(PlateState newState)
    {
        currentState = newState;
        incorrectPulseTimer = 0f;
        if (plateCollider != null)
            plateCollider.enabled = (currentState == PlateState.NotPressed);
        switch (currentState)
        {
            case PlateState.NotPressed:
                linkedGate.RemovePreasurePlateFromActivatedList((int)listPositionInGate);
                listPositionInGate = -1;
                materialInstance.color = notPressedColor;
                break;
            case PlateState.Pressed:
                activeTimer = resetDelay;
                listPositionInGate = linkedGate.AddPreasurePlateToActivatedList(this);
                materialInstance.color = pressedColor;
                break;
            case PlateState.Correct:
                correctSequence = true;
                materialInstance.color = correctSequenceColor;
                break;
            case PlateState.Incorrect:
                // Start with primary color, will pulsate in Update
                materialInstance.color = incorrectSequenceColorPrimary;
                break;
        }
    }

    void Update()
    {
        if(!correctSequence) activeTimer -= Time.deltaTime;

        model.localPosition = Vector3.Lerp(model.localPosition, 
            activeTimer > 0 ? new Vector3(0, -pressDepth, 0) : Vector3.zero, 
            Time.deltaTime * pressSpeed);

        // Pulsate color if in Incorrect state
        if (currentState == PlateState.Incorrect && materialInstance != null)
        {
            incorrectPulseTimer += Time.deltaTime;
            float pulse = (Mathf.Sin(incorrectPulseTimer * incorrectPulseSpeed) + 1f) * 0.5f; // 5 Hz pulsation
            materialInstance.color = Color.Lerp(incorrectSequenceColorPrimary, incorrectSequenceColorSecondary, pulse);
        }

        if(activeTimer < 0) SetState(PlateState.NotPressed);
    }

    void OnTriggerEnter(Collider other)
    {
        if(correctSequence)
            return;

        if (other.CompareTag("Player"))
        {
            SetState(PlateState.Pressed);
        }
    }
}
