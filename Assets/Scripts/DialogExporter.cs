using System.IO;
using System.Text;
using UnityEngine;
using XNode;

public class DialogueExporter : MonoBehaviour
{
    [Header("Old graph with old DialogNode")]
    public NodeGraph sourceGraph;

    [Header("Press this key to export")]
    public KeyCode exportKey = KeyCode.P;

    [Header("Output file name")]
    public string fileName = "dialog_export.txt";

    void Update()
    {
        if (Input.GetKeyDown(exportKey))
            Export();
    }

    void Export()
    {
        if (sourceGraph == null)
        {
            Debug.LogWarning("[Exporter] No sourceGraph assigned.");
            return;
        }

        var sb = new StringBuilder();
        int nodeCount = 0;

        foreach (var node in sourceGraph.nodes)
        {
            // Cast to your OLD DialogNode type (from main)
            var dn = node as DialogNode; // <-- old class on your main branch
            if (dn == null) continue;

            nodeCount++;
            sb.AppendLine($"# Node: {dn.name}");
            // If your old node had an ID, include it (remove if not present)
            // sb.AppendLine($"ID: {dn.nodeID}");

            if (!string.IsNullOrEmpty(dn.whostalking))
                sb.AppendLine($"Speaker: {dn.whostalking}");

            if (dn.dialogText != null)
            {
                for (int i = 0; i < dn.dialogText.Length; i++)
                {
                    string line = dn.dialogText[i] ?? "";
                    sb.AppendLine($"- {line}");
                }
            }

            sb.AppendLine();
        }

        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

        GUIUtility.systemCopyBuffer = sb.ToString();

        Debug.Log($"[Exporter] Exported {nodeCount} nodes to:\n{path}\n(Content also copied to clipboard)");
    }
}