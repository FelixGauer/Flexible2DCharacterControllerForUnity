using UnityEngine;

using UnityEngine;

public class AreaTriggerManager : MonoBehaviour
{
    public static AreaTriggerManager Instance;

    private string currentStringID;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void SetCurrentStringID(string id)
    {
        currentStringID = id;
    }

    public void ClearCurrentStringID(string id)
    {
        // Only clear if the ID matches currentStringID
        if (currentStringID == id)
            currentStringID = null;
    }

    public string GetCurrentStringID()
    {
        return currentStringID;
    }
}