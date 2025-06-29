using UnityEngine;
using UnityEngine.Serialization;

public class SquashAndStretch : MonoBehaviour
{
    public Transform sprite;
    public float stretch = 0.1f;
    [SerializeField] private Transform squashParent;

    
    private Rigidbody2D _rigidbody;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;

    
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _originalScale = sprite.transform.localScale;
        _originalPosition = sprite.localPosition;
 
        if(!squashParent)
            squashParent = new GameObject(string.Format("_squash_{0}", name)).transform;
    }
 
    private void Update()
    {
        sprite.parent = transform;
        sprite.localPosition = Vector3.zero;
        sprite.localRotation = Quaternion.identity;
 
        squashParent.localScale = Vector3.one;
        squashParent.position = transform.position;
 
        Vector3 velocity = _rigidbody.linearVelocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            squashParent.rotation = Quaternion.FromToRotation(Vector3.right, velocity);
        }
 
        var scaleX = 1.0f + (velocity.magnitude * stretch);
        var scaleY = 1.0f / scaleX;
        sprite.parent = squashParent;
        squashParent.localScale = new Vector3(scaleX, scaleY, 1.0f);
    }
}