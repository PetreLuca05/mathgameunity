using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogManager))]
public class DialogManagerEditor : Editor
{
    SerializedProperty participantsProp;
    SerializedProperty allDialogLinesProp;
    SerializedProperty skipCutsceneProp;

    void OnEnable()
    {
        participantsProp = serializedObject.FindProperty("participants");
        allDialogLinesProp = serializedObject.FindProperty("allDialogLines");
        skipCutsceneProp = serializedObject.FindProperty("skipCutscene");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default inspector except participants and allDialogLines and skipCutscene
        DrawPropertiesExcluding(serializedObject, "participants", "allDialogLines", "skipCutscene");


        // Render tooltip for skipCutscene
        EditorGUILayout.HelpBox("HOW TO TRIGGER THE QUEST AFTER DIALOG? - Attach the component QuestParent to the same GameObject as this DialogManager and after the dialog ends, the quest will start automatically.", MessageType.Info);
        EditorGUILayout.PropertyField(skipCutsceneProp);

        // In play mode, show a button to skip cutscene for testing
        DialogManager dialogManager = (DialogManager)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Skip Cutscene (Test)"))
            {
                dialogManager.SkipCutscene();
            }
        }

        // Participants
        EditorGUILayout.LabelField("Participants", EditorStyles.boldLabel);

        for (int i = 0; i < participantsProp.arraySize; i++)
        {
            var participantProp = participantsProp.GetArrayElementAtIndex(i);
            var participantTypeProp = participantProp.FindPropertyRelative("participantType");

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.PropertyField(participantTypeProp);

            EditorGUILayout.PropertyField(participantProp.FindPropertyRelative("camera"));

            // Only show characterModel if type is Other
            if ((DialogManager.ConversationParticipant.ParticipantType)participantTypeProp.enumValueIndex == DialogManager.ConversationParticipant.ParticipantType.Other)
            {
                EditorGUILayout.PropertyField(participantProp.FindPropertyRelative("characterModel"));
                EditorGUILayout.PropertyField(participantProp.FindPropertyRelative("name"));
                EditorGUILayout.PropertyField(participantProp.FindPropertyRelative("dialogPanelNameColor"));
            }

            // Remove button (red)
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove Participant"))
            {
                participantsProp.DeleteArrayElementAtIndex(i);
                GUI.backgroundColor = Color.white;
                break;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        // Add button (green)
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add Participant"))
        {
            participantsProp.InsertArrayElementAtIndex(participantsProp.arraySize);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialog Lines", EditorStyles.boldLabel);

        // Prepare participant names for dropdown
        string[] participantNames = new string[participantsProp.arraySize];
        for (int i = 0; i < participantsProp.arraySize; i++)
        {
            var participantProp = participantsProp.GetArrayElementAtIndex(i);
            var typeProp = participantProp.FindPropertyRelative("participantType");
            var nameProp = participantProp.FindPropertyRelative("name");
            if ((DialogManager.ConversationParticipant.ParticipantType)typeProp.enumValueIndex == DialogManager.ConversationParticipant.ParticipantType.Player)
            {
                participantNames[i] = "Player";
            }
            else
            {
                participantNames[i] = string.IsNullOrEmpty(nameProp.stringValue) ? "Other " + i : nameProp.stringValue;
            }
        }

        // Dialog lines
        for (int i = 0; i < allDialogLinesProp.arraySize; i++)
        {
            var dialogItemProp = allDialogLinesProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.PropertyField(dialogItemProp.FindPropertyRelative("dialog"));

            // Dropdown for speakerIndex
            var speakerIndexProp = dialogItemProp.FindPropertyRelative("speakerIndex");
            int currentIndex = speakerIndexProp.intValue;
            if (participantNames.Length > 0)
            {
                int newIndex = EditorGUILayout.Popup("Speaker", Mathf.Clamp(currentIndex, 0, participantNames.Length - 1), participantNames);
                if (newIndex != currentIndex)
                    speakerIndexProp.intValue = newIndex;
            }
            else
            {
                EditorGUILayout.HelpBox("No participants defined.", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(dialogItemProp.FindPropertyRelative("delayForNextLine"));
            EditorGUILayout.PropertyField(dialogItemProp.FindPropertyRelative("isFlipped"));
            EditorGUILayout.PropertyField(dialogItemProp.FindPropertyRelative("dialogPanelOffset"));

            // Remove dialog line button (red)
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Remove Dialog Line"))
            {
                allDialogLinesProp.DeleteArrayElementAtIndex(i);
                GUI.backgroundColor = Color.white;
                break;
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        // Add dialog line button (green)
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Add Dialog Line"))
        {
            allDialogLinesProp.InsertArrayElementAtIndex(allDialogLinesProp.arraySize);
        }
        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }
}