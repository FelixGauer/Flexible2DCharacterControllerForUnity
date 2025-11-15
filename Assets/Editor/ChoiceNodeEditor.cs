#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(ChoiceNode))]
public class ChoiceNodeEditor_Custom : NodeEditor
{
    // You can keep this slim if you like
    private const int NODE_WIDTH = 220;
    private const float OPTION_HEIGHT = 40f;

    public override int GetWidth()
    {
        return NODE_WIDTH;
    }

    public override void OnBodyGUI()
    {
        serializedObject.Update();

        var node = target as ChoiceNode;
        var whosProp = serializedObject.FindProperty("whostalking");
        var charProp = serializedObject.FindProperty("character_img");
        var bgProp = serializedObject.FindProperty("background_img");
        var colorProp = serializedObject.FindProperty("emotionColor");
        var telBoolProp = serializedObject.FindProperty("canTeleport");
        var telLocProp = serializedObject.FindProperty("teleportLocation");
        var promptProp = serializedObject.FindProperty("prompt");
        var optionsProp = serializedObject.FindProperty("options");

        // --- Standard fields (same order as DialogNode) ---
        NodeEditorGUILayout.PropertyField(whosProp);
        NodeEditorGUILayout.PropertyField(charProp);
        NodeEditorGUILayout.PropertyField(bgProp);
        NodeEditorGUILayout.PropertyField(colorProp);

        NodeEditorGUILayout.PropertyField(telBoolProp);
        NodeEditorGUILayout.PropertyField(telLocProp);

        // --- Prompt Text Area ---
        EditorGUILayout.Space(8);
        if (promptProp != null)
        {
            EditorGUILayout.PropertyField(promptProp, GUIContent.none);
        }

        // --- Options + their ports (port on its own line) ---
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

        if (optionsProp != null && optionsProp.isArray && node != null)
        {
            GUIStyle wrapStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };

            for (int i = 0; i < optionsProp.arraySize; i++)
            {
                var elem = optionsProp.GetArrayElementAtIndex(i);
                NodePort port = node.GetOutputPort($"choices {i}");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Option {i}", EditorStyles.miniBoldLabel);

                // Text for this option
                elem.stringValue = EditorGUILayout.TextArea(
                    elem.stringValue,
                    wrapStyle,
                    GUILayout.MinHeight(OPTION_HEIGHT)
                );

                // Compact buttons in a row
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+ Above"))
                {
                    optionsProp.InsertArrayElementAtIndex(i);
                    optionsProp.GetArrayElementAtIndex(i).stringValue = "";
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                if (GUILayout.Button("Del"))
                {
                    optionsProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                if (GUILayout.Button("+ Below"))
                {
                    optionsProp.InsertArrayElementAtIndex(i + 1);
                    optionsProp.GetArrayElementAtIndex(i + 1).stringValue = "";
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                // Port on its own line, aligned to the right
                if (port != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    NodeEditorGUILayout.PortField(new GUIContent("→"), port);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Option"))
            {
                int idx = optionsProp.arraySize;
                optionsProp.InsertArrayElementAtIndex(idx);
                optionsProp.GetArrayElementAtIndex(idx).stringValue = "";
            }
        }

        // --- Input port at the bottom ---
        EditorGUILayout.Space(6);
        var inputPort = target.GetInputPort("input");
        if (inputPort != null)
            NodeEditorGUILayout.PortField(inputPort);

        // --- Optional: defaultNext (kept, not drawn) ---
        /*
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Default Next", EditorStyles.boldLabel);
        var defaultNextPort = target.GetOutputPort("defaultNext");
        if (defaultNextPort != null)
            NodeEditorGUILayout.PortField(defaultNextPort);
        */

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
