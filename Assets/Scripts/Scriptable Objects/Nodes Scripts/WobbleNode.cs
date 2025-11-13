using UnityEngine;
using XNode;

[CreateNodeMenu("VN/Wobble Node")]
public class WobbleNode : DialogNode
{
    [Header("Wobble Settings")]
    public float wobbleDuration = 0.4f;  // total time of the wobble
    public float wobbleScale = 1.1f;  // how "big" the pulse gets (1.0 = no change)
    public int wobbleCycles = 1;     // how many in-out pulses

    public override void Play(DialogContext ctx, DialogManager runner)
    {
        // Normal dialog behavior (speaker, sprites, text, typing, etc.)
        base.Play(ctx, runner);

        // Add wobble on top
        runner.BeginWobble(this);
    }
}