using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Header("Interacting")]
    public Transform interactPanel;
    public TextMeshProUGUI interactPromptText;
    public TextMeshProUGUI interactKeyText;

    [Header("Inventory")]
    public Transform inventoryPanel;

    public void UpdateInventoryPanelLocation(Transform inventoryTarget, float lerpSpeed = 10f)
    {
        if (inventoryPanel == null)
            return;

        Vector3 targetScale = (inventoryTarget == null) ? Vector3.zero : Vector3.one;
        inventoryPanel.localScale = Vector3.Lerp(inventoryPanel.localScale, targetScale, Time.deltaTime * lerpSpeed);

        if (inventoryTarget != null)
        {
            Canvas canvas = inventoryPanel.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera != null)
            {
                Vector2 canvasPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    Camera.main.WorldToScreenPoint(inventoryTarget.position),
                    canvas.worldCamera,
                    out canvasPos
                );
                inventoryPanel.localPosition = canvasPos;
            }
            else
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(inventoryTarget.position);
                inventoryPanel.position = screenPos;
            }
        }
    }

    public void UpdateInteractPanel(Transform interactableTransform, string textToDisplay = "interact", string key = "E")
    {
        if (interactableTransform == null || interactPanel == null){
            interactPanel.localScale = Vector3.zero;
            return;
        }

        if(interactKeyText != null)
        {
            interactPromptText.text = textToDisplay;
            interactKeyText.text = key;
        }

        interactPanel.localScale = Vector3.one;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(interactableTransform.position);
        interactPanel.position = screenPos;
    }
}
