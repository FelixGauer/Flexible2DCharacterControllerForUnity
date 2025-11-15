using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    [Header("Jump to this node when player enters.")]
    public BaseStoryNode targetNode;  // <- direct reference

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.root.CompareTag("Player"))
        {
            var dialogManager = other.transform.root.GetComponent<DialogManager>();
            if (dialogManager == null) return;

            if (targetNode != null)
            {
                dialogManager.JumpToNode(targetNode);
            }
            else
            {
                Debug.LogWarning("[TriggerHandler] No targetNode assigned.");
            }
        }
    }
}