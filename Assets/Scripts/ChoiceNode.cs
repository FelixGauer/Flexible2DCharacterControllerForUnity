using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("VN/Choice Node")]
public class ChoiceNode : BaseStoryNode
{
    [Header("Speaker")]
    public string whostalking;

    [Header("UI")]
    [TextArea(2, 4)]
    public string prompt;

    [Header("Options (one line per choice)")]
    public List<string> options = new List<string>();

    [Output(dynamicPortList = true)]
    public BaseStoryNode[] choices;

    [Header("Visuals")]
    public Sprite character_img;
    public Sprite background_img;
    public CharacterColor emotionColor;

    public override void Play(DialogContext ctx, DialogManager runner)
    {
        // speaker
        ctx.SetSpeaker(whostalking);

        // visuals
        ctx.SetSprites(character_img, background_img);
        if (emotionColor != null)
            ctx.SetFrameColor(emotionColor.uiColor);

        // show text
        if (!string.IsNullOrEmpty(prompt))
            ctx.SetLine(prompt);

        runner.BeginChoice(this);
    }
    private void OnValidate()
    {
        // keep dynamic ports in sync with options
        if (choices == null || choices.Length != options.Count)
        {
            System.Array.Resize(ref choices, options.Count);
        }
    }
}
