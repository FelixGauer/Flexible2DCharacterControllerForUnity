using UnityEngine;
using XNode;

[CreateNodeMenu("VN/Notes Node")]
public class NotesNode : Node
{
    [Header("Notes (for author only)")]
    [TextArea(3, 10)]
    public string notes;

    [Header("Editor Settings")]
    public bool collapsed = false;      // if true, hide the text area
    public float editorHeight = 120f;   // height of the text area in the node
}
