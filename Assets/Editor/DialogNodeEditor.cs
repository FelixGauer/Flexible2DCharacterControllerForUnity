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

        // --- Draw your normal fields (unchanged) ---
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("nodeID"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("teleportLocation"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("whostalking"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("character_img"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("background_img"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("nodeQuestion"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("questionTexts"));
        NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("questionObjects"));

        // --- Custom drawing for dialogText[] as large text areas ---
        var linesProp = serializedObject.FindProperty("dialogText");
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Dialog Lines", EditorStyles.boldLabel);

        if (linesProp != null && linesProp.isArray)
        {
            GUIStyle wrapStyle = new GUIStyle(EditorStyles.textArea);
            wrapStyle.wordWrap = true;

            for (int i = 0; i < linesProp.arraySize; i++)
            {
                var elem = linesProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Line {i + 1}", EditorStyles.miniBoldLabel);

                elem.stringValue = EditorGUILayout.TextArea(
                    elem.stringValue,
                    wrapStyle,
                    GUILayout.MinHeight(100)
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


        // --- Input port ---
        NodeEditorGUILayout.PortField(target.GetInputPort("inputNode"));

        // --- Output ports (dynamic list) ---
        NodeEditorGUILayout.DynamicPortList(
            "nextNodes",
            typeof(DialogNode),
            serializedObject,
            NodePort.IO.Output,
            Node.ConnectionType.Multiple
        );

        serializedObject.ApplyModifiedProperties();
    }
}
#endif