using TMPro;
using UnityEngine;

public class DatingSim_Controller : MonoBehaviour
{
    public GameObject textbox;
    public GameObject background;
    public GameObject charImg;
    public TMP_Text dialogText;
    public string[] text_string;

    int currentDialogID;


    void Start()
    {
        //Set initial data? neccessary?
            
        SetDSActive();
        currentDialogID = 0;

    }

    void ContinueDialoge ()
    {
        currentDialogID++;
        dialogText.text = text_string[currentDialogID];

    }

    void SetDSActive()
    {
        background.SetActive(true);
        charImg.SetActive(true);
        textbox.SetActive(true);
        dialogText.text = text_string[currentDialogID];

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

        ContinueDialoge();

        }


    }
}
