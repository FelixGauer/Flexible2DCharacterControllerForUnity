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

    // ❌ NO [Output(dynamicPortList = true)] here anymore
    // We manage dynamic ports manually instead

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure list is not null
        if (options == null)
            options = new List<string>();

        // Sync dynamic ports with options count
        SyncChoicePorts();
    }

    private void SyncChoicePorts()
    {
        // 1) Remove extra ports
        // DynamicPorts is all ports created via AddDynamicOutput/AddDynamicInput
        var dynamicPorts = new List<NodePort>(DynamicPorts);
        foreach (var p in dynamicPorts)
        {
            // Our ports are named "choices 0", "choices 1", ...
            if (p.fieldName.StartsWith("choices "))
            {
                string indexStr = p.fieldName.Substring("choices ".Length);
                if (int.TryParse(indexStr, out int idx))
                {
                    if (idx >= options.Count)
                    {
                        RemoveDynamicPort(p);
                    }
                }
            }
        }

        // 2) Ensure one port per option
        for (int i = 0; i < options.Count; i++)
        {
            string portName = $"choices {i}";
            if (GetOutputPort(portName) == null)
            {
                AddDynamicOutput(
                    typeof(BaseStoryNode),
                    Node.ConnectionType.Multiple,
                    Node.TypeConstraint.None,
                    portName
                );
            }
        }
    }
#endif
}
