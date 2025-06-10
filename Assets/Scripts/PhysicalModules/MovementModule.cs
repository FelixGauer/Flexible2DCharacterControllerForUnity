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

    public void HandleMovement(Vector2 moveDirection, float speed, float acceleration, float deceleration)
    {
        _moveVelocity = _physicsContext.MoveVelocity;
			
        // Вычисление вектора направления перемноженного на скорость
        _targetVelocity = moveDirection != Vector2.zero
            ? new Vector2(moveDirection.x, 0f) * speed
            : Vector2.zero;

        // Вычисление ускорения или замедления игрока в воздухе или на земле
        float smoothFactor = moveDirection != Vector2.zero
            ? acceleration
            : deceleration;

        // Обработка позиции игрока по X
        _moveVelocity.x = Vector2.Lerp(_moveVelocity, _targetVelocity, smoothFactor * Time.fixedDeltaTime).x;
		
        _physicsContext.MoveVelocity = _moveVelocity;
    }
}