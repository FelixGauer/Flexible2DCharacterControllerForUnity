#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(DialogNode))]
public class DialogNodeEditor_Custom : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        // --- Current fields (kept simple & safe) ---
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("whostalking"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("character_img"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("background_img"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("emotionColor"));

        // Teleport options come from BaseStoryNode
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("canTeleport"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("teleportLocation"));

        // --- Dialog lines as word-wrapped TextAreas ---
        var linesProp = serializedObject.FindProperty("dialogText");
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Dialog Lines", EditorStyles.boldLabel);

        if (linesProp != null && linesProp.isArray)
        {
            GUIStyle wrapStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };

            for (int i = 0; i < linesProp.arraySize; i++)
            {
                var elem = linesProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Line {i + 1}", EditorStyles.miniBoldLabel);

                elem.stringValue = EditorGUILayout.TextArea(
                    elem.stringValue,
                    wrapStyle,
                    GUILayout.MinHeight(80)
                );

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Insert Above"))
                {
                    linesProp.InsertArrayElementAtIndex(i);
                    linesProp.GetArrayElementAtIndex(i).stringValue = "";
                    break;
                }
                if (GUILayout.Button("Remove"))
                {
                    linesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                if (GUILayout.Button("Add Below"))
                {
                    linesProp.InsertArrayElementAtIndex(i + 1);
                    linesProp.GetArrayElementAtIndex(i + 1).stringValue = "";
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Line"))
            {
                int idx = linesProp.arraySize;
                linesProp.InsertArrayElementAtIndex(idx);
                linesProp.GetArrayElementAtIndex(idx).stringValue = "";
            }
        }

        var inputPort = target.GetInputPort("input");
        if (inputPort != null)
            NodeEditorGUILayout.PortField(inputPort);

        // --- Ports: input(s) if you add later, and defaultNext output ---
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Next", EditorStyles.boldLabel);
        var defaultNextPort = target.GetOutputPort("defaultNext");
        if (defaultNextPort != null)
            NodeEditorGUILayout.PortField(defaultNextPort);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
