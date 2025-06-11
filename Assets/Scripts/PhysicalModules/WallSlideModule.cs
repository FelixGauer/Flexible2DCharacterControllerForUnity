using UnityEngine;

public class WallSlideModule
{
    private readonly PhysicsContext _physicsContext;
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly TurnChecker _turnChecker;
    private readonly CountdownTimer _wallJumpTimer;
	
    private Vector2 _moveVelocity;
    private bool _wasWallSliding;

    private bool IsFacingRight => _turnChecker.IsFacingRight;

    public WallSlideModule(PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, TurnChecker turnChecker, CountdownTimer wallJumpTimer) 
    {
        _physicsContext = physicsContext;
        _playerControllerStats = playerControllerStats;
        _turnChecker = turnChecker;
        _wallJumpTimer = wallJumpTimer;

    }
	
    // public void HandleWallInteraction(Vector2 _moveDirection, InputButtonState jumpInputButtonState)
    // {
    //     _moveVelocity = _physicsContext.MoveVelocity;
    //
    //     HandleWallSlide(_moveDirection);
		  //
    //     if (jumpInputButtonState.WasPressedThisFrame)
    //     {
    //         HandleWallJump(_moveDirection);
    //     }
		  //
    //     _physicsContext.MoveVelocity = _moveVelocity;
    //
    //     // if (_jumpKeyWasPressed)
    //     // {
    //     // 	HandleWallJump(Vector2 _moveDirection);
    //     // }
    // }

    // Обработка скольжения по стене WallSlide
    public Vector2 HandleWallSlide(Vector2 _moveVelocity, Vector2 _moveDirection)
    {
        // _moveVelocity = _physicsContext.MoveVelocity;
        // Плавное Изменение Y на стене,для скольжения 
        // Lerp зависит - 1. Начальная скорость скольжения (задается в Enter), 2 - макс. скорость скольжения, 3 - Deceleration скольжения
        _moveVelocity.y = Mathf.Lerp(_moveVelocity.y, -_playerControllerStats.WallSlideSpeedMax, _playerControllerStats.WallSlideDeceleration * Time.fixedDeltaTime); //FIXME

        HandleWallJumpTimer(_moveDirection);

        // Если ввод обратный вводу стены, ввод не равен 0, таймер не идет и на стене, запускаем таймер, отвечающий остаток времени на стене
        // Запуск таймера при попытке слезть со стены, помогает выполнить прыжок от стены
        if (_moveDirection.x != CalculateWallDirectionX() && _moveDirection.x != 0f && !_wallJumpTimer.IsRunning && _physicsContext.WasWallSliding)
        {
            _wallJumpTimer.Start();
        }

        // _physicsContext.MoveVelocity = _moveVelocity;
        
        // _physicsHandler2D.AddVelocity(_moveVelocity);
        
        return _moveVelocity;
    }

    // Метод управления таймером
    private void HandleWallJumpTimer(Vector2 _moveDirection)
    {
        // Управление состоянием таймера и скольжением по стене
        if (_wallJumpTimer.IsFinished)
        {
            // Персонаж продолжает скользить по стене, сбрасываем таймер
            if (_moveDirection.x == CalculateWallDirectionX() || _moveDirection.x == 0f)
            {
                _wallJumpTimer.Stop();
                _wallJumpTimer.Reset();
            }
            else // Если персонаж пытается отойти от стены, останавливаем скольжение
            {
                _physicsContext.WasWallSliding = false;
            }
        }
    }

    // Метод вычисляет направление стены по X
    public float CalculateWallDirectionX()
    {
        return IsFacingRight ? 1f : -1f;
    }

    // Применение прыжка со стены WallJump
    public void HandleWallJump(Vector2 _moveDirection)
    {
        // _moveVelocity = _physicsContext.MoveVelocity;

        // Расчет направление персонажа по X
        float wallDirectionX = CalculateWallDirectionX();
	
        if (_moveDirection.x == wallDirectionX) // Если ввод в сторону стены
        {
            // Прыжок вверх по стене
            _moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpClimb.x, _playerControllerStats.WallJumpClimb.y);
        }
        else if (_moveDirection.x == 0f) // Если ввод равен 0
        {
            // Прыжок от стены
            _moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpOff.x, _playerControllerStats.WallJumpOff.y);
        }
        else // Если ввод сторону от стены, обратную сторону
        {
            // Прыжок в сторону от стены
            _moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallLeap.x, _playerControllerStats.WallLeap.y);
        }
		
        // _physicsContext.MoveVelocity = _moveVelocity;
    }
    
    // Метод вызываемый при входе в состояние wallJump/Slide
	
    public void OnEnterWallSliding()
    {
        // Начальная скорость скольжения, x = 0 - персонаж падает вниз после окончания скольжения, то есть не сохраняет скорость
        // _physicsHandler2D.ResetVelocity();

        
        _moveVelocity = new Vector2(0f, -_playerControllerStats.StartVelocityWallSlide);
        _physicsContext.MoveVelocity = _moveVelocity;
		
        // Сброс таймера
        _wallJumpTimer.Reset();

        // Флаг скольжения по стене
        _physicsContext.WasWallSliding = true;

        // Обнуления максКолвоПрыжков, максКолвоРывков
        _physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps;
        _physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash;
    }

    // Метод вызываемый при выходе в состояние wallJump/Slide
    public void OnExitWallSliding()
    {
        // Остановка таймера
        _wallJumpTimer.Stop();
		
        // Сброс флага скольжения
        _physicsContext.WasWallSliding = false;
    }
	
	
}