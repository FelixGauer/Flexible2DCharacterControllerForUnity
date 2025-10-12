using System;
using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    public int nodeID;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.transform.root.CompareTag("Player"))
        {
            GameObject parent = other.gameObject.transform.root.gameObject;
            DialogManager dialogManager = parent.GetComponent<DialogManager>();
            dialogManager.SwitchNode(nodeID);
        }
    }
}
