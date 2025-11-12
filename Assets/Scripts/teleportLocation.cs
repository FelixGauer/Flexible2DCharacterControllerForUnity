using UnityEngine;

[ExecuteAlways]
public class TeleportLocation : MonoBehaviour
{
    [Header("Optional exact landing spot")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Just for editor visualization")]
    [SerializeField] private float gizmoRadius = 0.25f;

    /// <summary>World position the player should be teleported to.</summary>
    public Vector3 GetPosition()
    {
        return spawnPoint != null ? spawnPoint.position : transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GetPosition(), gizmoRadius);
    }
}