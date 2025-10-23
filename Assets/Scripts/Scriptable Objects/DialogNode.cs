using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;


[CreateAssetMenu(menuName = "VN/DialogNode")]
public class DialogNode : Node
{
    public int nodeID;

    public TeleportLocationAsset teleportLocation;

    public string[] dialogText;
    
    public Sprite character_img;
    public Sprite background_img;
    
    public bool nodeQuestion; //Once we impliement further, I will connect bool with other variables
    public List<string> questionTexts;
    public List<GameObject> questionObjects;

    [Input(backingValue = ShowBackingValue.Never)]
    public DialogNode inputNode;

    [Output(dynamicPortList = true)]
    public DialogNode[] nextNodes;


    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        if (port.fieldName == "nextNodes")
        {
            if (port.ConnectionCount > 0)
            {
                return port.GetConnection(0).node;
            }
            return null;
        }
        return null;
    }
}