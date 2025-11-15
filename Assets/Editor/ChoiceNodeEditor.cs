#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(ChoiceNode))]
public class ChoiceNodeEditor_Custom : NodeEditor
{
    // Global width for all ChoiceNodes – change once here if needed
    private const int NODE_WIDTH = 220;
    private const float OPTION_HEIGHT = 30f;

    public override int GetWidth()
    {
        return NODE_WIDTH;
    }

    public override void OnBodyGUI()
    {
        serializedObject.Update();

        // --- Standard fields (same order as DialogNode) ---
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("whostalking"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("character_img"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("background_img"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("emotionColor"));

        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("canTeleport"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("teleportLocation"));

        // --- Prompt text ---
        var promptProp = serializedObject.FindProperty("prompt");
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Prompt", EditorStyles.boldLabel);
        if (promptProp != null)
        {
            EditorGUILayout.PropertyField(promptProp, GUIContent.none);
        }

        // --- Options as text areas ---
        var optionsProp = serializedObject.FindProperty("options");
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

        if (optionsProp != null && optionsProp.isArray)
        {
            GUIStyle wrapStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };

            for (int i = 0; i < optionsProp.arraySize; i++)
            {
                var elem = optionsProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Option {i}", EditorStyles.miniBoldLabel);

                elem.stringValue = EditorGUILayout.TextArea(
                    elem.stringValue,
                    wrapStyle,
                    GUILayout.MinHeight(OPTION_HEIGHT)
                );

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Insert Above"))
                {
                    optionsProp.InsertArrayElementAtIndex(i);
                    optionsProp.GetArrayElementAtIndex(i).stringValue = "";
                    break;
                }
                if (GUILayout.Button("Remove"))
                {
                    optionsProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                if (GUILayout.Button("Add Below"))
                {
                    optionsProp.InsertArrayElementAtIndex(i + 1);
                    optionsProp.GetArrayElementAtIndex(i + 1).stringValue = "";
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Option"))
            {
                int idx = optionsProp.arraySize;
                optionsProp.InsertArrayElementAtIndex(idx);
                optionsProp.GetArrayElementAtIndex(idx).stringValue = "";
            }
        }

        // --- Node elements: dynamic outputs for choices ---
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Choice Outputs", EditorStyles.boldLabel);
        NodeEditorGUILayout.DynamicPortList(
            "choices",
            typeof(BaseStoryNode),
            serializedObject,
            NodePort.IO.Output,
            Node.ConnectionType.Multiple
        );

        // --- Input port at the bottom ---
        EditorGUILayout.Space(6);
        var inputPort = target.GetInputPort("input");
        if (inputPort != null)
            NodeEditorGUILayout.PortField(inputPort);

        // --- Optional: defaultNext port (currently commented out) ---
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
