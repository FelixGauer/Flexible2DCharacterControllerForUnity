using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DialogContext
{
    public TMP_Text speakerText;
    public TMP_Text dialogText;
    public Image charImage;
    public Image backgroundImage;
    public Image charFrame;
    public WritingAnimation writing;

    public void SetSpeaker(string name)
    {
        if (speakerText != null)
            speakerText.text = name ?? "";
    }

    public void SetLine(string text)
    {
        if (dialogText != null)
            dialogText.text = text ?? "";
    }

    public void SetSprites(Sprite character, Sprite background)
    {
        if (charImage != null)
        {
            if (character != null)
            {
                if (!charImage.gameObject.activeSelf) charImage.gameObject.SetActive(true);
                if (!charImage.enabled) charImage.enabled = true;
                charImage.sprite = character;
            }
            else
            {
                // Hide entirely if no character for this node
                if (charImage.enabled) charImage.enabled = false;
                if (charImage.gameObject.activeSelf) charImage.gameObject.SetActive(false);
                charImage.sprite = null;
            }
        }

        if (backgroundImage != null && background != null)
        {
            if (!backgroundImage.gameObject.activeSelf) backgroundImage.gameObject.SetActive(true);
            if (!backgroundImage.enabled) backgroundImage.enabled = true;
            backgroundImage.sprite = background;
        }
    }

    public void SetFrameColor(Color c)
    {
        if (charFrame == null) return;

        if (!charFrame.gameObject.activeSelf) charFrame.gameObject.SetActive(true);
        if (!charFrame.enabled) charFrame.enabled = true;

        if (c.a <= 0f) c.a = 1f; // avoid invisible frame
        charFrame.color = c;
    }
}