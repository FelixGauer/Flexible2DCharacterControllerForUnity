#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(DialogNode))]
public class DialogNodeEditor_Custom : NodeEditor
{
    // ⬇⬇⬇ EDIT THESE TWO NUMBERS ONCE ⬇⬇⬇
    private const int NODE_WIDTH = 220;   // width of the node in the graph
    private const float LINE_HEIGHT = 120f;  // min height of each dialog text area
    // ⬆⬆⬆ EDIT THESE TWO NUMBERS ONCE ⬆⬆⬆

    public override int GetWidth()
    {
        return NODE_WIDTH;
    }

    public override void OnBodyGUI()
    {
        serializedObject.Update();

        // --- Standard fields ---
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("whostalking"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("character_img"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("background_img"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("emotionColor"));

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("canTeleport"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("teleportLocation"));

        // --- Dialog lines as word-wrapped TextAreas ---
        var linesProp = serializedObject.FindProperty("dialogText");
        EditorGUILayout.Space(8);
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
                    GUILayout.MinHeight(LINE_HEIGHT) // ⬅ uses the global line height
                );

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+ Above"))
                {
                    linesProp.InsertArrayElementAtIndex(i);
                    linesProp.GetArrayElementAtIndex(i).stringValue = "";
                    break;
                }
                if (GUILayout.Button("Del"))
                {
                    linesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                if (GUILayout.Button("+ Below"))
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

        // Ports
        var inputPort = target.GetInputPort("input");
        if (inputPort != null)
            NodeEditorGUILayout.PortField(inputPort);

        EditorGUILayout.Space(6);
        var defaultNextPort = target.GetOutputPort("defaultNext");
        if (defaultNextPort != null)
            NodeEditorGUILayout.PortField(defaultNextPort);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif