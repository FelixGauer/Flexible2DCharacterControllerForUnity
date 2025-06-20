using UnityEngine;

public class MovementModule 
{
    public enum InterpolationType
    {
        MoveTowards,
        Lerp
    }
    
    private Vector2 _targetVelocity;
    // private Vector2 _moveVelocity;

    public MovementModule() { }
    
    public Vector2 HandleMovement(Vector2 moveVelocity, Vector2 moveDirection, float speed, float acceleration, float deceleration, InterpolationType interpolationType = InterpolationType.MoveTowards)
    {
        // Вычисление вектора направления перемноженного на скорость
        _targetVelocity = moveDirection != Vector2.zero
            ? new Vector2(moveDirection.x, 0f) * speed
            : Vector2.zero;

        // Вычисление ускорения или замедления игрока в воздухе или на земле
        float smoothFactor = moveDirection != Vector2.zero
            ? acceleration
            : deceleration;


        switch (interpolationType)
        {
            case InterpolationType.MoveTowards:
                moveVelocity.x = ApplyMoveTowards(moveVelocity, smoothFactor);
                break;
            case InterpolationType.Lerp:
                moveVelocity.x = ApplyLerp(moveVelocity, smoothFactor);
                break;
        }
        
        return moveVelocity;
        
        // Обработка позиции игрока по X
        // _moveVelocity.x = Vector2.Lerp(_moveVelocity, _targetVelocity, smoothFactor * Time.fixedDeltaTime).x; // Старый метод, сейчас использую MoveTowards
        //
        // if (Mathf.Abs(_moveVelocity.x) < 0.1f) 
        // {
        //     _moveVelocity.x = 0f;
        // }
        
        // moveVelocity.x = Mathf.MoveTowards(
        //     moveVelocity.x,
        //     _targetVelocity.x,
        //     smoothFactor * Time.fixedDeltaTime
        // );

        // moveVelocity.x = ApplyMoveTowards(moveVelocity, smoothFactor);
    }

    private float ApplyMoveTowards(Vector2 currentVelocity, float smoothFactor)
    {
        var moveVelocityX = Mathf.MoveTowards(
            currentVelocity.x,
            _targetVelocity.x,
            smoothFactor * Time.fixedDeltaTime
        );

        return moveVelocityX;
    }
    
    private float ApplyLerp(Vector2 currentVelocity, float smoothFactor) // При использовании нужно уменьшить значения акселерации в прыжке
    {
        var moveVelocityX = Vector2.Lerp(currentVelocity, _targetVelocity, smoothFactor * Time.fixedDeltaTime).x; // Старый метод, сейчас использую MoveTowards
        
        // if (Mathf.Abs(moveVelocityX) < 0.1f) 
        // {
        //     moveVelocityX = 0f;
        // }

        return moveVelocityX;
    }
}