using UnityEngine;

[ExecuteInEditMode]
public class TeleportLocationGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.cyan;
    public Vector3 gizmoSize = new Vector3(1f, 1f, 1f);

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, gizmoSize);
    }
}
