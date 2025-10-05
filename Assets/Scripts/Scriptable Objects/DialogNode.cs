using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;


[CreateAssetMenu(menuName = "VN/DialogNode")]
public class DialogNode : Node
{

    // stringID is for calling the correct dialognode from within the DS but also the JnR level
    public string stringID;

    // public string [] dialogString contains the actual dialog, this is not implemented

    [Input(backingValue = ShowBackingValue.Never)]
    public DialogNode inputNode;

    [Output(dynamicPortList = true)]
    public DialogNode[] nextNodes;


    public override object GetValue(NodePort port)
    {
        if (port.fieldName == "nextNodes")
        {
            if (port.ConnectionCount > 0)
            {
                DialogNode nextNode = port.GetConnection(0).node as DialogNode;
                if (nextNode != null)
                    return nextNode.stringID;  // Return stringID instead of node object
            }
            return null;
        }
        return null;
    }
}