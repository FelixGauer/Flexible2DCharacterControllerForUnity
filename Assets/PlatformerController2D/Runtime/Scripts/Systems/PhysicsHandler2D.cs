using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.Systems
{
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

        public Vector2 GetVelocity() => _rb.linearVelocity;

        public Vector2 GetLastAppliedVelocity() => _lastAppliedVelocity;

        void FixedUpdate()
        {
            // Debug.Log(_rb.linearVelocity.x);

            _rb.linearVelocity = _accumulatedVelocity;
            _lastAppliedVelocity = _accumulatedVelocity;
            _accumulatedVelocity = Vector2.zero;
        }
    }
}