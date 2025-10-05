using UnityEngine;

public class JNRTriggerArea : MonoBehaviour

{
    public string stringID;  // Directly assign the dialog node ID here

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);

        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            AreaTriggerManager.Instance.SetCurrentStringID(stringID);
            Debug.Log("Player entered area, stringID set to: " + stringID);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            AreaTriggerManager.Instance.ClearCurrentStringID(stringID);
    }
}
