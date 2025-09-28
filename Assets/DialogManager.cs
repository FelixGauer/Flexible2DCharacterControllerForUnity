using TMPro;
using UnityEngine;

public class DialogManager : MonoBehaviour

{
    // DialogAsset does not handle GameObject textbox, background or charImg, just contains the data to call it.

    public DialogAsset currentDialog;
    public TMP_Text dialogText;
    public string[] text_string;

    int currentDialogIndex = 0;


    void Start()
    {
        // set active UI elements (). This function would be called from the JnR level again.
        ShowCurrentline();

    }

    void ShowCurrentline()
    {
        dialogText.text = currentDialog.text_string[currentDialogIndex];
    }



    void ContinueDialoge()
    {
        currentDialogIndex++;
        if (currentDialogIndex < currentDialog.text_string.Length)
        {
            ShowCurrentline();
        }
        else if (currentDialog.nextAsset != null)
        {
            currentDialog = currentDialog.nextAsset;
            currentDialogIndex = 0;
            ShowCurrentline();
        }
        else
        {
            // Dialog ended
        }

}
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {

            ContinueDialoge();

        }

    }
}
