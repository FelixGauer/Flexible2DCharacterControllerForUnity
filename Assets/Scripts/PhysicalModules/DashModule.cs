using UnityEngine;

public class DashModule 
{
    private readonly PhysicsContext _physicsContext;
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CountdownTimer _dashTimer;
    private readonly TurnChecker _turnChecker;
    private readonly PhysicsHandler2D _physicsHandler2D;
	
    private Vector2 _moveVelocity;
    private Vector2 _dashDirection;
	
    private bool IsFacingRight => _turnChecker.IsFacingRight;

    public DashModule(PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, TurnChecker turnChecker, CountdownTimer dashTimer) 
    {
        _physicsContext = physicsContext;
        _playerControllerStats = playerControllerStats;
        _dashTimer = dashTimer;
        _turnChecker = turnChecker;
    }
    
    public Vector2 CurrentDashDirection => _dashDirection;
	
    public Vector2 HandleDash(Vector2 _moveVelocity)
    {
        _moveVelocity = _physicsContext.MoveVelocity;

        // Изменение скорости по оси X для совершения рывка
        _moveVelocity.x = _dashDirection.x * _playerControllerStats.DashVelocity;

        // Если скорость по y не равна 0, применяем рывок в оси Y
        if (_dashDirection.y != 0)
        {
            _moveVelocity.y = _dashDirection.y * _playerControllerStats.DashVelocity;
        }

        // Отмена рывка, если игрок проживаем противоположное направление
        // if (_dashDirection == -moveDirection)
        // {
        //     StopDash();
        // }

        // Применение гравитации во время рывка
        _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.DashGravityMultiplayer);
		
        _physicsContext.MoveVelocity = _moveVelocity;
        
        return _moveVelocity;
    }

    // Метод вызываемый при входе в состояние рывка
    public void StartDash(Vector2 moveDirection)
    {
        CalculateDashDirection(moveDirection);
        
        _dashTimer.Start(); // Запуск таймера рывка
        _physicsContext.NumberAvailableDash -= 1;
        _moveVelocity.y = 0f; // Сброс скорости по Y, для расчета правильного направления рывка
        _physicsContext.MoveVelocity = Vector2.zero;
    }

    // Метод вызываемый при выходе из состояния рывка
    public void StopDash()
    {
        _dashTimer.Stop(); // Остановка таймера
        _dashTimer.Reset(); // Сброс таймера
    }

    // Расчет направления рывка
    public void CalculateDashDirection(Vector2 moveDirection) // Убрать из FallState и вызывать сразу в EnterState
    {
        // _dashDirection = input.Direction;
        // _dashDirection = moveDirection;
		
        _dashDirection = GetClosestDirection(moveDirection); // Поиск ближайшего допустимого направления
    }
	
    // Метод для поиска ближайшего направления рывка
    private Vector2 GetClosestDirection(Vector2 targetDirection)
    {
        Vector2 closestDirection = Vector2.zero; // Начальное значение для ближайшего направления
        float minDistance = float.MaxValue;      // Минимальная дистанция для поиска ближайшего направления

        // Перебор всех допустимых направления в общем массиве направлений
        foreach (var dashDirection in _playerControllerStats.DashDirections)
        {
            float distance = Vector2.Distance(targetDirection, dashDirection);

            // Проверка на диагональное направление
            if (IsDiagonal(dashDirection))
            {
                distance = 1f;
            }
            // Если найдено близкое направление, обновляем ближайшее и минимальную дистанцию
            if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = dashDirection;
            }
        }

        // Если стоит на месте, применяем рывок в сторону поворота игрока, иначе в найденое ближайшее направление
        // return closestDirection == Vector2.zero ? (IsFacingRight ? Vector2.right : Vector2.left) : closestDirection;
        return closestDirection == Vector2.zero ? (IsFacingRight ? Vector2.right : Vector2.left) : closestDirection;

    }
	
    // Проверка является ли направление диагональным
    private bool IsDiagonal(Vector2 direction)
    {
        return Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1;
    }
}