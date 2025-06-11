using UnityEngine;

public interface IPhysicsHandler
{
    void AddVelocity(Vector2 vel);
}

public class PhysicsHandler2D : MonoBehaviour, IPhysicsHandler
{
    private Rigidbody2D _rb;
    private Vector2 _accumulatedVelocity;

    void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void AddVelocity(Vector2 vel) => _accumulatedVelocity += vel;

    public void ResetVelocity() => _accumulatedVelocity = Vector2.zero;
    public Vector2 GetVelocity() => _rb.linearVelocity;
    
    //TODO Добавить метод который будет возвращать true при == Vector2.Zero

    void FixedUpdate()
    {
        // Debug.Log(_rb.linearVelocity);
        _rb.linearVelocity = _accumulatedVelocity;
        _accumulatedVelocity = Vector2.zero;
    }
}