using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogManager : InteractParent
{
    [Tooltip("HOW TO TRIGGER THE QUEST AFTER DIALOG? - Attach the component QuestParent to the same GameObject as this DialogManager and after the dialog ends, the quest will start automatically.")]
    [Header("Cutscene Skipping")]
    public bool skipCutscene = false;

    [System.Serializable]
    public class ConversationParticipant
    {
        public enum ParticipantType { Player, Other }
        public ParticipantType participantType;
        public GameObject camera;
        public string name;
        public PlayerModel characterModel;
        public Color dialogPanelNameColor = Color.gray;
    }

    [System.Serializable]
    public struct DialogItem
    {
        public string dialog;
        public int speakerIndex;
        public float delayForNextLine;
        public bool isFlipped;
        public Vector2 dialogPanelOffset;
    }
    [Header("Dialog Settings")]
    public GameObject camerasParent;
    public float typeSpeed = 0.05f;

    public DialogItem[] allDialogLines;

    public Transform pointToTeleportPlayerTo;

    public float delayBetweenLines = 1f;
    public GameObject DialogCanvas;

    public GameObject DialogEndPanel;

    public List<ConversationParticipant> participants = new List<ConversationParticipant>();
    int currentParticipantIndex = 0;

    ConversationParticipant CurrentParticipant =>
        (participants.Count > 0 && currentParticipantIndex >= 0 && currentParticipantIndex < participants.Count)
            ? participants[currentParticipantIndex]
            : null;

    DialogPanel dialogPanel;
    Coroutine dialogCoroutine;
    Coroutine typewriterCoroutine;

    void Start()
    {
        dialogPanel = GetComponentInChildren<DialogPanel>(true);
        DialogEndPanel.SetActive(false);
        DialogCanvas.SetActive(false);
        camerasParent.SetActive(false);

        for (int i = 0; i < participants.Count; i++)
        {
            if (participants[i].camera != null)
                participants[i].camera.SetActive(false);
        }
    }

    public override void Interact()
    {
        base.Interact();
        // skipCutscene = false;
        GameManager.instance.GetComponentInPlayer<PlayerManager>().SetBehaviourState(BehaviourState.inDialog);
        GameManager.instance.GetComponentInPlayer<PlayerManager>().transform.position = pointToTeleportPlayerTo.position;
        GameManager.instance.GetComponentInPlayer<PlayerManager>().transform.rotation = pointToTeleportPlayerTo.rotation;
        GameManager.instance.GetComponentInPlayer<InteractManager>().ClearFloundInteractable();
        StartDialog();
    }

    public void StartDialog()
    {
        if (allDialogLines == null || allDialogLines.Length == 0)
        {
            Debug.LogWarning("No dialog lines to start.");
            return;
        }
        if (dialogCoroutine != null)
            StopCoroutine(dialogCoroutine);
        dialogCoroutine = StartCoroutine(DisplayDialogSequence());
    }

    private System.Collections.IEnumerator DisplayDialogSequence()
    {
        DialogCanvas.SetActive(true);
        camerasParent.SetActive(true);

        if (DialogEndPanel != null)
            DialogEndPanel.SetActive(false);

        for (int i = 0; i < allDialogLines.Length; i++)
        {
            if (skipCutscene)
            {
                break;
            }
            var line = allDialogLines[i];
            currentParticipantIndex = line.speakerIndex;
            FocusOnCurrentParticipantCamera();
            float calculatedTypeTime = typeSpeed * Mathf.Max(1, line.dialog.Length);
            ShowDialogLine(i, calculatedTypeTime);
            yield return new WaitForSeconds(calculatedTypeTime);
            float totalDelay = delayBetweenLines + line.delayForNextLine;
            if (totalDelay > 0f)
                yield return new WaitForSeconds(totalDelay);
        }
        dialogCoroutine = null;
        if (DialogEndPanel != null)
            DialogEndPanel.SetActive(true);
        // Call UnityEvent or function after dialog ends
    }
    // Call this method to skip the cutscene/dialog
    public void SkipCutscene()
    {
        skipCutscene = true;
        // Optionally, immediately end dialog and close UI
        if (dialogCoroutine != null)
        {
            StopCoroutine(dialogCoroutine);
            dialogCoroutine = null;
        }
        // Disable all participant cameras
        if (camerasParent != null)
        {
            foreach (Transform child in camerasParent.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        BTN_FINISH();
    }

    private void OnDialogEnd()
    {
        QuestParent questToStart =  transform.parent.GetComponentInChildren<QuestParent>();
        if (questToStart != null && !questToStart.questStarted)
        {
            questToStart.StartQuest();
        }
        // Disable all participant cameras
        if (camerasParent != null)
        {
            foreach (Transform child in camerasParent.transform)
            {
                child.gameObject.SetActive(false);
            }
            camerasParent.SetActive(false);
        }
    }

    private void FocusOnCurrentParticipantCamera()
    {
        // Disable all participant cameras first
        if (camerasParent != null)
        {
            foreach (Transform child in camerasParent.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        // Enable only the current participant's camera
        if (participants != null && currentParticipantIndex >= 0 && currentParticipantIndex < participants.Count)
        {
            var cam = participants[currentParticipantIndex].camera;
            if (cam != null)
                cam.SetActive(true);
        }
    }

    private void ShowDialogLine(int index, float calculatedTypeTime)
    {
        if (index < 0 || index >= allDialogLines.Length)
            return;
        var line = allDialogLines[index];
        var participant = participants[line.speakerIndex];

        // If participant is Player, get name, dialogPanelPosition, dialogPanelNameColor from PlayerManager
        string displayName = participant.name;
        Color displayNameColor = participant.dialogPanelNameColor;
        if (participant.participantType == ConversationParticipant.ParticipantType.Player)
        {
            var playerManager = GameManager.instance.GetComponentInPlayer<PlayerManager>();
            displayName = playerManager.nameOfPlayer;
            displayNameColor = playerManager.playerDialogNameColor;
        }

        //Debug.Log($"{displayName}: {line.dialog}");

        if (dialogPanel != null)
        {
            dialogPanel.SetDialogBoxData(displayName, line.isFlipped, displayNameColor);
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterEffectDialogPanel(dialogPanel, line.dialog, calculatedTypeTime));
        }
    }

    private System.Collections.IEnumerator TypewriterEffectDialogPanel(DialogPanel dialogPanel, string fullText, float totalTime)
    {
        if (dialogPanel == null) yield break;
        dialogPanel.SetDialogText("");
        float delay = totalTime / Mathf.Max(fullText.Length, 1);
        for (int i = 0; i < fullText.Length; i++)
        {
            dialogPanel.SetDialogText(dialogPanel.dialogText.text + fullText[i]);
            yield return new WaitForSeconds(delay);
        }
        dialogPanel.SetDialogText(fullText);
    }

    void Update()
    {
        if(!isInteractingWithMe) return;

        if (dialogPanel != null && CurrentParticipant != null)
        {
            Transform panelPos = CurrentParticipant.characterModel?.dialogPanelTransform;
            if (CurrentParticipant.participantType == ConversationParticipant.ParticipantType.Player)
            {
                var playerManager = GameManager.instance.GetComponentInPlayer<PlayerManager>();
                panelPos = playerManager.GetComponentInChildren<PlayerModel>().dialogPanelTransform;
            }
            if (panelPos != null)
                UpdateDialogPanel(dialogPanel.transform, panelPos);
        }
    }

    public void UpdateDialogPanel(Transform dialogPanel, Transform destination)
    {
        if (dialogPanel == null || destination == null){
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(destination.position);
        // Apply offset in screen space (2D)
        Vector3 offset = Vector3.zero;
        if (allDialogLines != null && allDialogLines.Length > 0 && dialogCoroutine != null)
        {
            // Try to get the current dialog line's offset
            int currentLineIndex = 0;
            if (typewriterCoroutine != null)
                currentLineIndex = Mathf.Clamp(currentParticipantIndex, 0, allDialogLines.Length - 1);
            offset = allDialogLines[currentLineIndex].dialogPanelOffset;
        }
        // Only use x and y for 2D offset
        screenPos.x += offset.x;
        screenPos.y += offset.y;
        dialogPanel.localScale = Vector3.one;
        dialogPanel.position = screenPos;
    }

    public override void QuitInteract()
    {
        base.QuitInteract();
        DialogCanvas.SetActive(false);
        GameManager.instance.GetComponentInPlayer<PlayerManager>().SetBehaviourState(BehaviourState.Default);
    }

    public void BTN_FINISH()
    {
        QuitInteract();
        OnDialogEnd();
    }

    public void BTN_RECAP()
    {
        StartDialog();
    }
}