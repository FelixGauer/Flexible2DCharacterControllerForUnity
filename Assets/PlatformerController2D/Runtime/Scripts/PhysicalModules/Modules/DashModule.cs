using UnityEngine;

public class DashModule 
{
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CountdownTimer _dashTimer;
    private readonly TurnChecker _turnChecker;
    private readonly CollisionsChecker _collisionsChecker;

    private float _numberAvailableDash;
	
    private Vector2 _moveVelocity;
    private Vector2 _dashDirection;
	
    private bool IsFacingRight => _turnChecker.IsFacingRight;

    public DashModule(PlayerControllerStats playerControllerStats, TurnChecker turnChecker, CountdownTimer dashTimer, CollisionsChecker collisionsChecker) 
    {
        _playerControllerStats = playerControllerStats;
        _turnChecker = turnChecker;
        _dashTimer = dashTimer;
        _collisionsChecker = collisionsChecker;
        

        // collisionsChecker.OnGroundTouched += ResetNumberAvailableDash;
        // collisionsChecker.OnWallTouched += ResetNumberAvailableDash;
        // collisionsChecker.OnWallTouched += ResetNumberAvailableDashOnWall;
        
        collisionsChecker.OnGroundTouched += () => ResetNumberAvailableDash(isWall: false);
        collisionsChecker.OnWallTouched += () => ResetNumberAvailableDash(isWall: true);

        
        // if (playerControllerStats.WallDashRecovery) _collisionsChecker.OnWallTouched += ResetNumberAvailableDash;

    }
	
    public Vector2 HandleDash()
    {
        _moveVelocity.x = _dashDirection.x * _playerControllerStats.DashVelocity;

        if (_dashDirection.y != 0)
        {
            _moveVelocity.y = _dashDirection.y * _playerControllerStats.DashVelocity;
        }

        return _moveVelocity;
    }

    public void CheckForDirectionChange(Vector2 moveDirection)
    {
        if (_dashDirection == -moveDirection)
            StopDash();
    }

    public void StartDash(Vector2 moveDirection)
    {
        CalculateDashDirection(moveDirection);
        
        _dashTimer.Start();
        if (!_collisionsChecker.IsGrounded) _numberAvailableDash -= 1;
        _moveVelocity.y = 0f; 
    }

    public void StopDash()
    {
        _dashTimer.Stop(); 
        _dashTimer.Reset(); 
    }
    
    public bool CanDash() // FIXME Thinking
    {
        return _numberAvailableDash > 0f;
    }

    // public void ResetNumberAvailableDash() // FIXME Thinking
    // {
    //     _numberAvailableDash = _playerControllerStats.MaxNumberDash;
    // }
    //
    // public void ResetNumberAvailableDashOnWall() // FIXME Thinking
    // {
    //     if (_playerControllerStats.WallDashRecovery)
    //         _numberAvailableDash = _playerControllerStats.MaxNumberDash;
    //     else
    //         _numberAvailableDash = 0f;
    // }
    
    private void ResetNumberAvailableDash(bool isWall)
    {
        if (isWall && !_playerControllerStats.ResetNumberDashOnWall)
        {
            _numberAvailableDash = 0f;
        }
        else
        {
            _numberAvailableDash = _playerControllerStats.MaxNumberDash;
        }
    }

    private void CalculateDashDirection(Vector2 moveDirection)
    {
        _dashDirection = GetClosestDirection(moveDirection);
    }
	
    // Метод для поиска ближайшего направления рывка
    private Vector2 GetClosestDirection(Vector2 targetDirection)
    {
        Vector2 closestDirection = Vector2.zero;
        float minDistance = float.MaxValue;

        foreach (var dashDirection in _playerControllerStats.DashDirections)
        {
            float distance = Vector2.Distance(targetDirection, dashDirection);
        
            // ИСПРАВЛЕНО: Приоритет диагональным направлениям
            if (IsDiagonal(dashDirection) && IsDiagonal(targetDirection))
            {
                distance *= 0.5f; // Делаем диагональные направления более приоритетными
            }
        
            if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = dashDirection;
            }
        }

        return closestDirection == Vector2.zero ? 
            (IsFacingRight ? Vector2.right : Vector2.left) : closestDirection;
    }
	
    // Проверка является ли направление диагональным
    private bool IsDiagonal(Vector2 direction)
    {
        return Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1;
    }
}