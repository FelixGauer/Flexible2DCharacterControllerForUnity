using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;

public class DialogManager : MonoBehaviour
{
    // DialogAsset does not handle GameObject textbox, background or charImg, just contains the data to call it.

    public GameObject dsUI;
    public GameObject questionPanel;
    // those will have multiples at some point. Right now I want them to show correctly

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
        ShowCurrentlineAndArt();
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

    void GetNextNode(int nodeID)
    {
        if (elementIndex < currentNode.dialogText.Length - 1)
        {
            elementIndex++;
            ShowCurrentlineAndArt();
            return;
        }
        
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
        dsUI.SetActive(isDSactive);
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