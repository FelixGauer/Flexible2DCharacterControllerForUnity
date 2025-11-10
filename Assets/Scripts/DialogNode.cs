using UnityEngine;


[CreateNodeMenu("VN/Dialogue Node")]
public class DialogNode : BaseStoryNode
{
    [Header("Dialogue")]
    public string whostalking;
    public string[] dialogText;

    [Header("Visuals")]
    public Sprite character_img;
    public Sprite background_img;
    public CharacterColor emotionColor;

    public int LineCount => dialogText != null ? dialogText.Length : 0;

    public string GetLine(int index)
    {
        if (dialogText == null || index < 0 || index >= dialogText.Length) return string.Empty;
        return dialogText[index] ?? string.Empty;
    }

    public override void Play(DialogContext ctx, DialogManager runner)
    {
        ctx.SetSpeaker(whostalking);
        ctx.SetSprites(character_img, background_img);
        if (emotionColor != null) ctx.SetFrameColor(emotionColor.uiColor);
        runner.BeginDialogue(this);
    }
}