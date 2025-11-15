#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogManager))]
public class DialogManagerEditor : Editor
{
    // Foldout state (per inspector session)
    private bool showContext = true;
    private bool showChoiceUI = true;
    private bool showTeleportIndicator = true;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Grab the properties we want to group
        SerializedProperty contextProp = serializedObject.FindProperty("context");
        SerializedProperty dialogGraphProp = serializedObject.FindProperty("dialogGraph");

        SerializedProperty questionPanelProp = serializedObject.FindProperty("questionPanel");
        SerializedProperty choiceButtonProp = serializedObject.FindProperty("choiceButtonPrefab");

        SerializedProperty phoneIconProp = serializedObject.FindProperty("phoneIcon");
        SerializedProperty phoneHiddenPosProp = serializedObject.FindProperty("phoneHiddenPos");
        SerializedProperty phoneShownPosProp = serializedObject.FindProperty("phoneShownPos");
        SerializedProperty phoneSlideDurProp = serializedObject.FindProperty("phoneSlideDuration");

        // --- Context foldout ---
        showContext = EditorGUILayout.Foldout(showContext, "Context", true);
        if (showContext)
        {
            EditorGUI.indentLevel++;
            if (dialogGraphProp != null)
                EditorGUILayout.PropertyField(dialogGraphProp);
            if (contextProp != null)
                EditorGUILayout.PropertyField(contextProp);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
        }

        // --- Choice UI foldout ---
        showChoiceUI = EditorGUILayout.Foldout(showChoiceUI, "Choice UI", true);
        if (showChoiceUI)
        {
            EditorGUI.indentLevel++;
            if (questionPanelProp != null)
                EditorGUILayout.PropertyField(questionPanelProp);
            if (choiceButtonProp != null)
                EditorGUILayout.PropertyField(choiceButtonProp);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
        }

        // --- Teleport Indicator foldout ---
        showTeleportIndicator = EditorGUILayout.Foldout(showTeleportIndicator, "Teleport Indicator", true);
        if (showTeleportIndicator)
        {
            EditorGUI.indentLevel++;
            if (phoneIconProp != null)
                EditorGUILayout.PropertyField(phoneIconProp);
            if (phoneHiddenPosProp != null)
                EditorGUILayout.PropertyField(phoneHiddenPosProp);
            if (phoneShownPosProp != null)
                EditorGUILayout.PropertyField(phoneShownPosProp);
            if (phoneSlideDurProp != null)
                EditorGUILayout.PropertyField(phoneSlideDurProp);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(8);
        }

        // Draw all remaining properties as usual
        DrawPropertiesExcluding(
            serializedObject,
            "context",
            "dialogGraph",
            "questionPanel",
            "choiceButtonPrefab",
            "phoneIcon",
            "phoneHiddenPos",
            "phoneShownPos",
            "phoneSlideDuration"
        );

        serializedObject.ApplyModifiedProperties();
    }
}
#endif