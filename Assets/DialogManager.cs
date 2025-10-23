using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using XNode;

public class DialogManager : MonoBehaviour
{
    // DialogAsset does not handle GameObject textbox, background or charImg, just contains the data to call it.

    private CanvasGroup dsGroup;
    public GameObject dsUI;
    public GameObject questionPanel;
    // those will have multiples at some point. Right now I want them to show correctly

    public GameObject playerForLocation;

    public GameObject charIMG;
    public GameObject backgroundIMG;

    public WritingAnimation writingAnimation;

    public TMP_Text dialogText;
    public NodeGraph dialogGraph;
    public DialogNode currentNode;
    public int elementIndex = 0;
    private bool isDSactive = true;

    int currentDialogIndex = 0;
    public int nodeId = 1;

    void Start()
    {
        // set active UI elements (). This function would be called from the JnR level again.
        elementIndex = 0;
        currentNode = dialogGraph.nodes[0] as DialogNode;

        dsGroup = dsUI.GetComponent<CanvasGroup>();
        if (!dsGroup) dsGroup = dsUI.AddComponent<CanvasGroup>();

        ShowCurrentlineAndArt();
        teleportAnchor.DebugDumpRegistry();
    }

    void ShowCurrentlineAndArt()
    {

        if (currentNode == null)
            return;
        CheckHasQuestions();

        if (elementIndex < currentNode.dialogText.Length)
        {
            string line = currentNode.dialogText[elementIndex];
            writingAnimation.StartTyping(line);
            UpdateImages();
        }
        else
        {
            nodeId++;
            GetNextNode(nodeId);
        }

    }

    void UpdateImages()
    {
        Image charImageComponent = charIMG.GetComponent<Image>();
        Image backgroundImageComponent = backgroundIMG.GetComponent<Image>();

        if (charImageComponent != null && currentNode.character_img != null)
            charImageComponent.sprite = currentNode.character_img;

        if (backgroundImageComponent != null)
            backgroundImageComponent.sprite = currentNode.background_img;

    }

    void CheckHasQuestions()
    {
        if (currentNode == null)
            return;

        bool hasQuestions = currentNode.nodeQuestion && currentNode.questionTexts != null && currentNode.questionTexts.Count > 0;

        // Assume you have a UI container GameObject that holds your question buttons, e.g. questionPanel
        // Enable it if there are questions, disable if none.

        questionPanel.SetActive(hasQuestions);

        if (hasQuestions)
        {
            // Populate your buttons here based on questionTexts...
            // For now, just a placeholder for clarity.
            ShowQuestionButtons(currentNode.questionTexts);
        }
        else
        {
            // If disabling, clear buttons or UI as needed
            ClearQuestionButtons();
        }
    }

    void ClearQuestionButtons()
    {

        //Disable Question UI elements

    }

    void ShowQuestionButtons(List<string> questions)
    {

        //Enable Question UI elements
        // This is just activating the elements for now, not updating the text or moving to different nodes

    }

    public void GetNextNode(int nodeID)
    {
        Debug.Log("GetNextNode called with nodeID: " + nodeID);
        if (elementIndex < currentNode.dialogText.Length - 1)
        {
            elementIndex++;
            ShowCurrentlineAndArt();
            return;
        }

        SwitchNode(nodeID);
    }

    public void SwitchNode(int nodeID)
    {
        if (nodeID != -1)
        {
            foreach (var node in dialogGraph.nodes)
            {
                if (node is DialogNode dialogNode && dialogNode.nodeID == nodeID)
                {
                    currentNode = dialogNode;
                    elementIndex = 0;
                    ShowCurrentlineAndArt();
                    return;
                }
            }

            Debug.LogWarning($"Node with ID {nodeID} not found in the graph.");
            return;
        }

        NodePort port = currentNode.GetOutputPort("nextNodes 0");
        if (port == null || port.ConnectionCount == 0)
            return; // End of dialog flow

        currentNode = port.GetConnection(0).node as DialogNode;
        elementIndex = 0;
        ShowCurrentlineAndArt();
    }

    void SwitchGame()
    {
        isDSactive = !isDSactive;


        if (!isDSactive)  // Leaving dialog, entering JnR
        {

            ApplyDsUiVisibility(false);

            if (currentNode != null && currentNode.teleportLocation != null)
            {

                Debug.Log($"[DialogManager] SwitchGame: current node '{currentNode?.name}', asset = '{currentNode?.teleportLocation?.name}'");
                if (teleportAnchor.TryGet(currentNode.teleportLocation, out var anchor))
                {

                    var target = anchor.GetPosition();
                    Debug.Log($"[DialogManager] Teleporting player '{playerForLocation.name}' to {target}");
                    playerForLocation.transform.position = target;
                    Debug.Log($"[DialogManager] Player now at {playerForLocation.transform.position}");

                    playerForLocation.transform.position = anchor.GetPosition();

                    // (optional) if you use Rigidbody2D, zero velocity:
                    // var rb = playerForLocation.GetComponent<Rigidbody2D>();
                    // if (rb) rb.velocity = Vector2.zero;
                }
                else
                {
                    Debug.LogWarning($"No TeleportAnchor found in scene for asset '{currentNode.teleportLocation.name}'.");
                }
            }
        }
        else
        {
            ApplyDsUiVisibility(true);
        }
   
    }

    void ApplyDsUiVisibility(bool on)
    {
        if (!dsGroup) return;
        dsGroup.alpha = on ? 1f : 0f;
        dsGroup.interactable = on;
        dsGroup.blocksRaycasts = on;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (writingAnimation.IsTyping())
            {
                writingAnimation.SkipTyping();
            }
            else
            {
                elementIndex++;
                ShowCurrentlineAndArt();
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            SwitchGame();
        }
    }
}