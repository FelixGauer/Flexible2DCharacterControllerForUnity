using TMPro;
using UnityEngine;
using XNode;

public class DialogManager : MonoBehaviour

{
    // DialogAsset does not handle GameObject textbox, background or charImg, just contains the data to call it.

    public GameObject dsUI;
    
    public TMP_Text dialogText;
    public NodeGraph dialogGraph;
    public DialogNode currentNode;

    private bool isDSactive = true;

    int currentDialogIndex = 0;

    void Start()
    {
        // set active UI elements (). This function would be called from the JnR level again.
        currentNode = dialogGraph.nodes[0] as DialogNode;
        ShowCurrentline();

    }

    void ShowCurrentline()
    {
        if (currentNode != null)
            dialogText.text = currentNode.dialogText;
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
        isDSactive = !isDSactive;
        dsUI .SetActive(isDSactive);


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

