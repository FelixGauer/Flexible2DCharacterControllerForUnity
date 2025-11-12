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

    private void OnChoiceSelected(ChoiceNode node, int index)
    {
        BaseStoryNode next = null;
        var port = node.GetOutputPort($"choices {index}");
        if (port != null && port.ConnectionCount > 0)
            next = port.GetConnection(0).node as BaseStoryNode;

        ClearChoiceUI();
        activeChoice = null;

        if (next != null) AdvanceTo(next);
        else CompleteNode();
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
        activeDialogue = null;
        activeChoice = node;
        ClearChoiceUI();

        if (questionPanel != null) questionPanel.SetActive(true);

        for (int i = 0; i < node.options.Count; i++)
        {
            Button btn = Instantiate(choiceButtonPrefab, questionPanel.transform);
            spawnedButtons.Add(btn);

            // label
            var label = btn.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (label) label.text = node.options[i];

            // capture index for listener
            int idx = i;
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"[Choice] Button {idx} clicked");
                btn.interactable = false;
                var img = btn.GetComponent<UnityEngine.UI.Image>();
                if (img) img.color = new Color(1f, 1f, 1f, 0.6f);
                OnChoiceSelected(node, idx);
            });
        }

        // ensure layout updates this frame
        var rt = questionPanel.transform as RectTransform;
        if (rt) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
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

    public void BeginAnimation(AnimationNode node)
    {
        activeDialogue = null;
        activeChoice = null;
        StopAllCoroutines();
        StartCoroutine(PlaySlideIn(node));
    }

    private System.Collections.IEnumerator PlaySlideIn(AnimationNode node)
    {
        // Ensure the char image is visible and set sprite
        if (context.charImage != null)
        {
            var go = context.charImage.gameObject;
            if (!go.activeSelf) go.SetActive(true);
            context.charImage.enabled = true;
            if (node.character_img != null)
                context.charImage.sprite = node.character_img;

            // Cache target position, set start position, then lerp to target
            RectTransform rt = context.charImage.rectTransform;
            Vector2 target = rt.anchoredPosition;
            Vector2 start = target + node.fromOffset;
            rt.anchoredPosition = start;

            float t = 0f;
            float d = Mathf.Max(0.01f, node.duration);
            while (t < d)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / d);
                rt.anchoredPosition = Vector2.Lerp(start, target, p);
                yield return null;
            }
            rt.anchoredPosition = target;
        }

        // Advance using your normal next-node logic
        CompleteNode();
    }

}



