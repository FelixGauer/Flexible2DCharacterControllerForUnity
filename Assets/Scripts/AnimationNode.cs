using UnityEngine;
using XNode;

[CreateNodeMenu("VN/Animation Node")]
public class AnimationNode : BaseStoryNode
{
    [Header("Visuals")]
    public Sprite character_img;
    public CharacterColor emotionColor;

    [Header("Slide In")]
    public Vector2 fromOffset = new Vector2(-800f, 0f); // start offset (px) from the target
    public float duration = 0.4f;                       // seconds

    public override void Play(DialogContext ctx, DialogManager runner)
    {
        // Clear dialog text; set visuals (frame color optional)
        ctx.SetLine(string.Empty);
        if (emotionColor != null) ctx.SetFrameColor(emotionColor.uiColor);

        // Hand off to manager to animate
        runner.BeginAnimation(this);
    }
}