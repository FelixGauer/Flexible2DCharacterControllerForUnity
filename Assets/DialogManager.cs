using TMPro;
using UnityEngine;
using XNode;

public class DialogManager : MonoBehaviour

{
    // DialogAsset does not handle GameObject textbox, background or charImg, just contains the data to call it.

    public TMP_Text dialogText;
    public NodeGraph dialogGraph;
    public DialogNode currentNode;

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

        var port = currentNode.GetOutputPort("nextNodes");
        if (port.ConnectionCount == 0)
            return;

        currentNode = port.GetConnection(0).node as DialogNode;
        ShowCurrentline();

    }

        void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                ContinueDialoge();
            }

        }
    }

