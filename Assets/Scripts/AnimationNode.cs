using UnityEngine;
using XNode;

[CreateNodeMenu("VN/Animation Node")]
public class AnimationNode : BaseStoryNode
{
    [Header("Visuals")]
    public Sprite character_img;
    public Sprite background_img;
    public CharacterColor emotionColor;

    [Header("Slide In Settings")]
    public Vector2 fromOffset = new Vector2(-800f, 0f);
    public float slideDuration = 0.4f;
    public float holdDuration = 1.0f;

    public override void Play(DialogContext ctx, DialogManager runner)
    {
        // clear text
        ctx.SetLine(string.Empty);

        // background image update
        if (background_img != null)
            ctx.SetSprites(null, background_img);

        // apply sprite frame + color
        if (character_img != null)
            ctx.SetSprites(character_img, null); // only overrides character

        if (emotionColor != null)
            ctx.SetFrameColor(emotionColor.uiColor);

        // start animation
        runner.BeginAnimation(this);
    }

}