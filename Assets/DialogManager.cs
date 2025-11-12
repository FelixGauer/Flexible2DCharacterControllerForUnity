using System.Collections.Generic;   // <-- for List<>// if you reference TMP_Text in code here
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;   // for Button, Image, etc.
using XNode;

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

    private ChoiceNode activeChoice;
    private readonly System.Collections.Generic.List<Button> spawnedChoiceButtons
        = new System.Collections.Generic.List<Button>();

    [Header("Choice UI")]
    public GameObject questionPanel;
    public Button choiceButtonPrefab;
    private List<Button> spawnedButtons = new List<Button>();


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

        if (Input.GetMouseButtonDown(0))
        {
            var data = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            if (results.Count > 0)
            {
                Debug.Log("[Raycast] Top hits:");
                foreach (var r in results) Debug.Log($" - {r.gameObject.name} (order={r.sortingOrder})");
            }
            else
            {
                Debug.Log("[Raycast] No UI hit");
            }
        }

        // block VN click-advance when a choice is active
        if (activeChoice == null && activeDialogue != null && Input.GetMouseButtonDown(0))
        {
            if (context.writing.IsTyping()) context.writing.SkipTyping();
            else
            {
                lineIndex++;
                if (lineIndex < activeDialogue.LineCount) ShowLine();
                else { activeDialogue = null; CompleteNode(); }
            }
        }

        if (currentNode != null && currentNode.canTeleport && Input.GetKeyDown(KeyCode.K))
            SwitchGame();
    }

    public void AdvanceTo(BaseStoryNode next)
    {
        if (next == null)
        {
            Debug.LogWarning("AdvanceTo called with null. Staying on current node.");
            return;
        }
        currentNode = next;
        PlayCurrent();
    }
    public void CompleteNode()
    {
        if (currentNode == null)
        {
            Debug.Log("No current node.");
            return;
        }

        // 1) defaultNext port
        NodePort def = currentNode.GetOutputPort("defaultNext");
        if (def != null && def.ConnectionCount > 0)
        {
            var next = def.GetConnection(0).node as BaseStoryNode;
            AdvanceTo(next);
            return;
        }

        // 2) first connected OUTPUT port (fallback)
        foreach (var port in currentNode.Ports)
        {
            if (port.direction == NodePort.IO.Output && port.ConnectionCount > 0)
            {
                var next = port.GetConnection(0).node as BaseStoryNode;
                if (next != null)
                {
                    AdvanceTo(next);
                    return;
                }
            }
        }

        // 3) no outputs => end
        Debug.Log("End of story – no connected outputs.");
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

    private void OnChoiceSelected(int index)
    {
        // get port: "choices 0", "choices 1", ...
        BaseStoryNode next = null;

        if (activeChoice != null)
        {
            NodePort port = activeChoice.GetOutputPort($"choices {index}");
            if (port != null && port.ConnectionCount > 0)
            {
                next = port.GetConnection(0).node as BaseStoryNode;
            }
        }

        ClearChoiceUI();
        activeChoice = null;

        if (next == null && currentNode != null)
        {
            // fallback to defaultNext logic
            CompleteNode();
            return;
        }

        AdvanceTo(next);
    }

    private void ClearChoiceUI()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
            if (spawnedButtons[i]) Destroy(spawnedButtons[i].gameObject);

        spawnedButtons.Clear();

        if (questionPanel != null) questionPanel.SetActive(false);
    }

        public void BeginChoice(ChoiceNode node)
        {
        
        if (dsGroup != null)
        {
            dsGroup.alpha = 1f;
            dsGroup.interactable = true;        // <- force clicks allowed
            dsGroup.blocksRaycasts = true;
        }

            activeDialogue = null;
            activeChoice = node;
            ClearChoiceUI();

            if (questionPanel != null) questionPanel.SetActive(true);

            // Show one button for test
            if (node.options.Count > 0)
            {
                Button btn = Instantiate(choiceButtonPrefab, questionPanel.transform);
                spawnedButtons.Add(btn);

                // set label
                var label = btn.GetComponentInChildren<TMPro.TMP_Text>(true);
                if (label == null)
                {
                    Debug.LogError("[Choice] No TMP_Text found under the button prefab. Check prefab hierarchy.");
                }
                else
                {
                    label.text = node.options[0];
                    // DEBUG: confirm text we set
                    Debug.Log($"[Choice] Set label to: {label.text}");
                }

                // DEBUG: confirm we created it
                Debug.Log($"[Choice] Spawned button: {btn.name} under {questionPanel.name}");
                Debug.Log($"[Choice] BeginChoice for node: {node.name}");
                Debug.Log($"[Choice] Spawned {btn.name}, label found={(label != null)} under panel {questionPanel.name}");
                if (label) label.text = node.options.Count > 0 ? node.options[0] : "(no option 0)";
                btn.onClick.AddListener(() => { Debug.Log("[Choice] Button clicked"); OnTestChoice(node); });

                // click listener with visual feedback
                btn.onClick.AddListener(() =>
                {
                    Debug.Log($"[Choice] Button listeners: {btn.onClick.GetPersistentEventCount()} + runtime");
                    Debug.Log("[Choice] Button clicked");
                    btn.interactable = false;
                    var img = btn.GetComponent<UnityEngine.UI.Image>();
                    if (img) img.color = new Color(1f, 1f, 1f, 0.6f);
                    OnTestChoice(node);
                });
            }
        }

    private void OnTestChoice(ChoiceNode node)
    {
        BaseStoryNode next = null;
        var port = node.GetOutputPort("choices 0");
        if (port != null && port.ConnectionCount > 0)
            next = port.GetConnection(0).node as BaseStoryNode;

        ClearChoiceUI();
        activeChoice = null;

        if (next != null) AdvanceTo(next);
        else CompleteNode();
    }

}



