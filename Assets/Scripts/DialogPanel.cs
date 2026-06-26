using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : MonoBehaviour
{   
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;

    public void SetDialogBoxData(string name, bool flipped, Color nameColor)
    {
        if (nameText != null)
        {
            nameText.text = name;
        }

        Image img = nameText.transform.parent.GetComponentInChildren<Image>();
        if (img != null)
        {
            //Debug.Log("Setting name color to: " + nameColor);
            img.color = nameColor;
        } else {
            Debug.LogWarning("No Image component found in parent of nameText.");
        }
    }

    public void SetDialogText(string text)
    {
        if (dialogText != null)
        {
            dialogText.text = text;
        }
    }
}
