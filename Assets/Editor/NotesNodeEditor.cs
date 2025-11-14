#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(NotesNode))]
public class NotesNodeEditor_Custom : NodeEditor
{
    public override int GetWidth()
    {
        // Fixed width for notes; tweak this if you want wider/narrower note boxes
        return 320;
    }

    public override void OnBodyGUI()
    {
        serializedObject.Update();

        var notesProp = serializedObject.FindProperty("notes");
        var collapsedProp = serializedObject.FindProperty("collapsed");
        var editorHeightProp = serializedObject.FindProperty("editorHeight");

        // Title / header
        EditorGUILayout.LabelField("Notes", EditorStyles.boldLabel);

        // Collapsible toggle
        if (collapsedProp != null)
        {
            collapsedProp.boolValue = EditorGUILayout.ToggleLeft("Collapse", collapsedProp.boolValue);
        }

        // Height control
        if (editorHeightProp != null)
        {
            editorHeightProp.floatValue = Mathf.Clamp(
                EditorGUILayout.FloatField("Text Height", editorHeightProp.floatValue),
                40f, 600f
            );
        }

        EditorGUILayout.Space(4);

        // Text area (only if not collapsed)
        if (collapsedProp == null || !collapsedProp.boolValue)
        {
            float h = editorHeightProp != null ? editorHeightProp.floatValue : 120f;

            GUIStyle wrapStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };

            EditorGUILayout.BeginVertical("box");
            if (notesProp != null)
            {
                notesProp.stringValue = EditorGUILayout.TextArea(
                    notesProp.stringValue,
                    wrapStyle,
                    GUILayout.MinHeight(h)
                );
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Notes collapsed.", MessageType.None);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif