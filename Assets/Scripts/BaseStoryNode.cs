using UnityEngine;
using XNode;

public abstract class BaseStoryNode : Node
{
    // NEW: input port (no backing value)
    [Input(backingValue = ShowBackingValue.Never)]
    public BaseStoryNode input;

    [Header("Story Navigation")]
    [Output] public BaseStoryNode defaultNext;

    [Header("Teleport")]
    public TeleportLocationAsset teleportLocation;
    public bool canTeleport = false;

    public abstract void Play(DialogContext ctx, DialogManager runner);
}