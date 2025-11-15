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

    [Header("Options (one per choice)")]
    public List<string> options = new List<string>();

    [Output(dynamicPortList = true)]
    public BaseStoryNode[] choices;

    [Header("Visuals")]
    public Sprite character_img;
    public Sprite background_img;
    public CharacterColor emotionColor;

    public override void Play(DialogContext ctx, DialogManager runner)
    {
        ctx.SetSpeaker(whostalking);
        ctx.SetSprites(character_img, background_img);
        if (emotionColor != null) ctx.SetFrameColor(emotionColor.uiColor);
        if (!string.IsNullOrEmpty(prompt)) ctx.SetLine(prompt);

        runner.BeginChoice(this);
    }

    // 🔑 This keeps ports in sync with the options list
    private void OnValidate()
    {
        if (options == null) options = new List<string>();

        if (choices == null || choices.Length != options.Count)
        {
            System.Array.Resize(ref choices, options.Count);
        }
    }
}
