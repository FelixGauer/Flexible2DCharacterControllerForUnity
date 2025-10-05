using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class boxDrawer : MonoBehaviour
{
    public Color gizmoColor = new Color(0f, 1f, 0f, 0.25f); // semi-transparent green

    private BoxCollider2D boxCollider;

    void OnDrawGizmos()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(boxCollider.bounds.center, boxCollider.bounds.size);
    }
}

