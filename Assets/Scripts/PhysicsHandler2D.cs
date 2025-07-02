using System;
using UnityEngine;

public interface IPhysicsHandler
{
    void AddVelocity(Vector2 vel);
}

public class PhysicsHandler2D : MonoBehaviour, IPhysicsHandler
{
    private Rigidbody2D _rb;
    private Vector2 _accumulatedVelocity;
    private Vector2 _lastAppliedVelocity;

    void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void AddVelocity(Vector2 vel) => _accumulatedVelocity += vel;

    // public void ResetVelocity() => _rb.linearVelocity = Vector2.zero;
    // public void ResetVelocity() => _accumulatedVelocity = new Vector2(_accumulatedVelocity.x, 0f);

    public Vector2 GetVelocity() => _rb.linearVelocity;
    public Vector2 GetLastAppliedVelocity() => _lastAppliedVelocity; // Новый метод

    
    //TODO Добавить метод который будет возвращать true при == Vector2.Zero

    void FixedUpdate()
    {
        _rb.linearVelocity = _accumulatedVelocity;
        _lastAppliedVelocity = _accumulatedVelocity;
        _accumulatedVelocity = Vector2.zero;
        
        // Debug.Log(_rb.linearVelocity);
    }
}