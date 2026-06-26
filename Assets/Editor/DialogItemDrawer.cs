using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogManager.DialogItem))]
public class DialogItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw dialog, speaker, delayForNextLine, and isFlipped fields
        var dialogRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        var speakerRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
        var delayRect = new Rect(position.x, position.y + 2 * (EditorGUIUtility.singleLineHeight + 2), position.width, EditorGUIUtility.singleLineHeight);
        var isFlippedRect = new Rect(position.x, position.y + 3 * (EditorGUIUtility.singleLineHeight + 2), position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(dialogRect, property.FindPropertyRelative("dialog"));

        // Get the DialogManager instance
        DialogManager dialogManager = property.serializedObject.targetObject as DialogManager;
        var participants = dialogManager != null ? dialogManager.participants : null;

        var speakerIndexProp = property.FindPropertyRelative("speakerIndex");
        int selectedIndex = speakerIndexProp.intValue;
        string[] options = new string[0];
        if (participants != null && participants.Count > 0)
        {
            options = new string[participants.Count];
            for (int i = 0; i < participants.Count; i++)
            {
                options[i] = participants[i].name;
            }
            int newIndex = EditorGUI.Popup(speakerRect, "Speaker", selectedIndex, options);
            if (newIndex >= 0 && newIndex < participants.Count)
            {
                speakerIndexProp.intValue = newIndex;
            }
        }
        else
        {
            EditorGUI.LabelField(speakerRect, "Speaker", "No participants found");
        }

        EditorGUI.PropertyField(delayRect, property.FindPropertyRelative("delayForNextLine"));
        EditorGUI.PropertyField(isFlippedRect, property.FindPropertyRelative("isFlipped"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 4 * (EditorGUIUtility.singleLineHeight + 2);
    }
}
