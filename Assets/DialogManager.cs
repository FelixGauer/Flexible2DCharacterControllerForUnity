using UnityEngine;
using XNode; // only needed if you keep using ports

public class DialogManager : MonoBehaviour
{
    public DialogContext context;
    public NodeGraph dialogGraph;
    private BaseStoryNode currentNode;

    // For dialogue typing
    private DialogNode activeDialogue;
    private int lineIndex = 0;

    // UI visibility toggle (teleport behavior)
    public CanvasGroup dsGroup;
    public GameObject playerForLocation;

    void Start()
    {
        // pick first node in the graph (or expose a specific start node later)
        currentNode = dialogGraph != null && dialogGraph.nodes.Count > 0
            ? dialogGraph.nodes[0] as BaseStoryNode
            : null;

        PlayCurrent();
    }

    public void PlayCurrent()
    {
        if (currentNode == null) return;

        // Nodes call back into this via Play()
        currentNode.Play(context, this);
    }

    // --- Called from DialogueNode.Play() ---
    public void BeginDialogue(DialogNode node)
    {
        activeDialogue = node;
        lineIndex = 0;

        if (activeDialogue.LineCount > 0)
            ShowLine();
        else
            CompleteNode();
    }

    void ShowLine()
    {
        string text = activeDialogue.GetLine(lineIndex);
        context.writing.StartTyping(text); // removed callback
    }


    void Update()
    {
        // click to advance lines
        if (activeDialogue != null && Input.GetMouseButtonDown(0))
        {
            if (context.writing.IsTyping())
            {
                context.writing.SkipTyping();
            }
            else
            {
                lineIndex++;
                if (lineIndex < activeDialogue.LineCount)
                {
                    ShowLine();
                }
                else
                {
                    activeDialogue = null;
                    CompleteNode();
                }
            }
        }

        // teleport toggle
        if (currentNode != null && currentNode.canTeleport && Input.GetKeyDown(KeyCode.K))
            SwitchGame();
    }

    public void CompleteNode()
    {
        // Try xNode output first
        var port = currentNode?.GetOutputPort("defaultNext");
        if (port != null && port.ConnectionCount > 0)
        {
            currentNode = port.GetConnection(0).node as BaseStoryNode;
            PlayCurrent();
            return;
        }

        Debug.Log("End of story – no next connected.");
    }


// Simplified teleport for now
void SwitchGame()
    {
        if (currentNode.teleportLocation != null && playerForLocation != null)
        {
            if (teleportAnchor.TryGet(currentNode.teleportLocation, out var anchor))
            {
                playerForLocation.transform.position = anchor.GetPosition();
            }
            else
            {
                Debug.LogWarning($"No TeleportAnchor found for '{currentNode.teleportLocation.name}'.");
            }
        }

        if (dsGroup != null)
        {
            bool show = !(dsGroup.alpha > 0f);
            dsGroup.alpha = show ? 1f : 0f;
            dsGroup.interactable = show;
            dsGroup.blocksRaycasts = show;
        }
    }
}
