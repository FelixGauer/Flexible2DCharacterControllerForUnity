using UnityEngine;

public class MovementModule 
{
    private Vector2 _targetVelocity;
    private Vector2 _moveVelocity;
    private readonly PhysicsContext _physicsContext;

    public MovementModule(PhysicsContext physicsContext) 
    {
        _physicsContext = physicsContext;
    }
    
    public Vector2 HandleMovement(Vector2 _moveVelocity, Vector2 moveDirection, float speed, float acceleration, float deceleration)
    {
        // _moveVelocity.x = _physicsHandler2D.GetVelocity().x;
			
        // Вычисление вектора направления перемноженного на скорость
        _targetVelocity = moveDirection != Vector2.zero
            ? new Vector2(moveDirection.x, 0f) * speed
            : Vector2.zero;

        // Вычисление ускорения или замедления игрока в воздухе или на земле
        float smoothFactor = moveDirection != Vector2.zero
            ? acceleration
            : deceleration;

        // Обработка позиции игрока по X
        // _moveVelocity.x = Vector2.Lerp(_moveVelocity, _targetVelocity, smoothFactor * Time.fixedDeltaTime).x; // Старый метод, сейчас использую MoveTowards
        //
        // if (Mathf.Abs(_moveVelocity.x) < 0.1f) 
        // {
        //     _moveVelocity.x = 0f;
        // }
        
        _moveVelocity.x = Mathf.MoveTowards(
            _moveVelocity.x,
            _targetVelocity.x,
            smoothFactor * Time.fixedDeltaTime
        );
        
        // _physicsContext.MoveVelocity = _moveVelocity;
        
        // _physicsHandler2D.AddVelocity(_moveVelocity);
        
        return _moveVelocity;
    }
}