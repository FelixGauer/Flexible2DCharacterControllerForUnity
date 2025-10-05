using TMPro;
using UnityEngine;
using XNode;

public class DialogManager : MonoBehaviour

{
    // DialogAsset does not handle GameObject textbox, background or charImg, just contains the data to call it.

    public GameObject dsUI;


    public DialogNode startNode;

    public TMP_Text dialogText;
    public NodeGraph dialogGraph;
    public DialogNode currentNode;

    private AreaTriggerManager areaTriggerManager;

    private bool isDSactive = true;

    int currentDialogIndex = 0;

    void Start()
    {
        // set active UI elements (). This function would be called from the JnR level again.
        areaTriggerManager = AreaTriggerManager.Instance;
        currentNode = startNode;
        ShowCurrentline();

    }

    void ShowCurrentline()
    {
        if (currentNode != null)
            dialogText.text = currentNode.stringID;
    }

    void ContinueDialoge()
    {

        // change so:
        // Instead of 1 String, go through all Strings in the array, THEN go to the next node
        if (isDSactive)
        {

            NodePort port = currentNode.GetOutputPort("nextNodes 0");
            if (port == null || port.ConnectionCount == 0)
                return;

            currentNode = port.GetConnection(0).node as DialogNode;
            ShowCurrentline();
        }
    }

    void SwitchGame()
    {
        if (!isDSactive)  // Going from JnR -> DS
        {
            CallStringIDfromArea();  // Get the stringID corresponding to player's current trigger
                                     // Enable DS UI etc.
            isDSactive = true;
            dsUI.SetActive(true);
            // Disable JnR player and UI here as needed
        }
        else  // Going from DS -> JnR
        {
            isDSactive = false;
            dsUI.SetActive(false);
            // Enable JnR gameplay, disable DS UI etc.
        }
    }

    void CallStringIDfromArea()
    {
        string stringID = areaTriggerManager.GetCurrentStringID();  // Your code to get current area ID

        DialogNode node = FindNodeByID(stringID);
        if (node != null)
        {
            currentNode = node;
            ShowCurrentline();
        }
        else
        {
            Debug.LogWarning("No node found for ID: " + stringID);
        }
    }

    DialogNode FindNodeByID(string id)
    {
        if (string.IsNullOrEmpty(id) || dialogGraph == null)
            return null;

        foreach (Node node in dialogGraph.nodes)
        {
            DialogNode dialogNode = node as DialogNode;
            if (dialogNode != null && dialogNode.stringID == id)
                return dialogNode;
        }

        return null;
    }

    void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                ContinueDialoge();
            }

            if (Input.GetKeyDown(KeyCode.K)) 
            {
                SwitchGame();
            }

        }
    }

