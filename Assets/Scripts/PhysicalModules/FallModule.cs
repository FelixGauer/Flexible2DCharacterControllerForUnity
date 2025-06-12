using UnityEngine;

public class FallModule 
{
    public FallModule(PhysicsContext physicsContext, Rigidbody2D rigidbody, CollisionsChecker collisionsChecker, PlayerControllerStats playerControllerStats, CountdownTimer jumpBufferTimer)
    {
        _rigidbody = rigidbody;
        _collisionsChecker = collisionsChecker;
        _playerControllerStats = playerControllerStats;
		
        _jumpBufferTimer = jumpBufferTimer;

        // _jumpBufferTimer = new CountdownTimer(playerControllerStats.BufferTime);

        _physicsContext = physicsContext;
    }
	
    private readonly Rigidbody2D _rigidbody;
    private readonly CollisionsChecker _collisionsChecker;
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CountdownTimer _jumpBufferTimer;
    

    private readonly PhysicsContext _physicsContext;
    
    private readonly PhysicsHandler2D _physicsHandler2D;

    private Vector2 _moveVelocity;
    private bool _coyoteUsable;

    private bool _isHeld;

    public void BufferJump(InputButtonState jumpState)
    {
        if (jumpState.WasPressedThisFrame) { _jumpBufferTimer.Start(); }
    }
    
    public void RequestVariableJump(InputButtonState jumpState)
    {
        // if (_jumpBufferTimer.IsRunning && jumpState.WasReleasedThisFrame)
        //     _physicsContext.VariableJumpHeight = true;
    }

    public void SetHoldState(bool isHeld) => _isHeld = isHeld;
    
    public Vector2 HandleFalling(Vector2 _moveVelocity)
    {
        // _moveVelocity = _physicsContext.MoveVelocity;
        
        // _moveVelocity.y = _physicsHandler2D.GetVelocity().y;
        
        float gravityMultiplier;

        // 1) Если «висим» на верхней точке прыжка (ниже порога hang-time):
        if (Mathf.Abs(_rigidbody.linearVelocity.y) < _playerControllerStats.jumpHangTimeThreshold)
        {
            gravityMultiplier = _playerControllerStats.jumpHangGravityMult;
        }
        // 2) Если ещё держат кнопку прыжка — уменьшаем силу гравитации (более мягкий подъем):
        else if (_isHeld)
        {
            gravityMultiplier = _playerControllerStats.JumpGravityMultiplayer;
        }
        // 3) Иначе (мы либо в свободном падении, либо отпустили кнопку — максимальная гравитация):
        else
        {
            gravityMultiplier = _playerControllerStats.FallGravityMultiplayer;
        }

        // Применяем гравитацию
        _moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, gravityMultiplier);

        // Ограничиваем максимальную скорость падения:
        _moveVelocity.y = Mathf.Clamp(_moveVelocity.y, -_playerControllerStats.maxFallSpeed, 50f); // TODO В отдельный метод

        // _physicsContext.MoveVelocity = _moveVelocity;
        
        // _physicsHandler2D.AddVelocity(_moveVelocity);

        return _moveVelocity;
    }

    // Метод вызываемый при выходе из состояния падения
    public void OnExitFall() // FIXME 
    {
        _moveVelocity = Vector2.zero;
    // Когда персонаж оказывается на земле, вернуть все флаги, которые обновляются на земле
    // Также используется для конкретной реализации буферного прыжка в StateMachine
    // if (_collisionsChecker.IsGrounded) _coyoteUsable = true;;
        if (_collisionsChecker.IsGrounded) HandleGround();
    }
	
    private void HandleGround()
    {
        _physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
        _physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash;
    
        // _coyoteUsable = true; // Установка флага разрешающего делать койот прыжок
    
        // _moveVelocity.y = _playerControllerStats.GroundGravity; // Гравитация на земле
    }
    // public void HandleFalling(InputButtonState jumpState)
    // {
    //     _moveVelocity = _physicsContext.MoveVelocity;
    //     // Проверка на удар головой об платформу
    //     // BumpedHead(); //FIXME
    //
    //     // Запуск таймера прыжка в падении
    //     // if (jumpState.WasPressedThisFrame) { _jumpBufferTimer.Start(); }
    //     // Сохранение переменной для буферизации минимального прыжка
    //     // if (_jumpBufferTimer.IsRunning && jumpState.WasReleasedThisFrame) { _physicsContext.VariableJumpHeight = true; } //FIXME
    //
    //     // Применнение гравитации
    //     // Гравитация в верхней точки прыжыка
    //     if (Mathf.Abs(_rigidbody.linearVelocity.y) < _playerControllerStats.jumpHangTimeThreshold)
    //     {
    //         _moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.jumpHangGravityMult);
    //
    //     } // Гравитация в прыжке (Гравитация если удерживается кнопка прыжка)
    //     else if (jumpState.IsHeld)
    //     {
    //         _moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.JumpGravityMultiplayer);
    //
    //     } // Гравитация в падении
    //     else
    //     {
    //         _moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.FallGravityMultiplayer);
    //     }
    //
    //     // Ограничение максимальной скорости падения
    //     _moveVelocity.y = Mathf.Clamp(_moveVelocity.y, -_playerControllerStats.maxFallSpeed, 50f);
		  //
    //     _physicsContext.MoveVelocity = _moveVelocity;
		  //
    //     // ApplyMovement();
    // }
}