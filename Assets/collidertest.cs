using UnityEngine;

public class collidertest : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger fired by: " + other.gameObject.name);
    }

}
