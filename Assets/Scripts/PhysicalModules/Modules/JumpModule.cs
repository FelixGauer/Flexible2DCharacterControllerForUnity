using UnityEngine;

public class JumpModule
{
    public JumpModule(CollisionsChecker collisionsChecker, PlayerControllerStats playerControllerStats, CountdownTimer jumpBufferTimer, CountdownTimer jumpCoyoteTimer)
    {
        _collisionsChecker = collisionsChecker;
        _playerControllerStats = playerControllerStats;

        _jumpCoyoteTimer = jumpCoyoteTimer;
        _jumpBufferTimer = jumpBufferTimer;

        _numberAvailableJumps = playerControllerStats.MaxNumberJumps;
        
        _collisionsChecker.OnGroundTouched += ResetNumberAvailableJumps;

        _collisionsChecker.OnWallTouched += ResetNumberAvailableJumps;
        
        _collisionsChecker.OnGroundLeft += StartCoyoteTime;
    }
    
    public event System.Action OnMultiJump; // Покинул землю (был на земле -> не на земле)


    private readonly CollisionsChecker _collisionsChecker;
    private readonly PlayerControllerStats _playerControllerStats;

    private readonly CountdownTimer _jumpCoyoteTimer;
    private readonly CountdownTimer _jumpBufferTimer;

    private Vector2 _moveVelocity;
    private bool _positiveMoveVelocity;
    private float _numberAvailableJumps;

    private InputButtonState _jumpState;

    private bool _jumpKeyReleased;
    private bool _wasPressedThisFrame;
    private bool _isHeld;

    private bool _shouldExecuteBufferJump;
    
    public void HandleInput(InputButtonState jumpState)
    {
        if (jumpState.WasPressedThisFrame) _wasPressedThisFrame = true;
        if (jumpState.WasReleasedThisFrame) _jumpKeyReleased = true;
        _isHeld = jumpState.IsHeld;

        if (_jumpBufferTimer.IsRunning)
        {
            _shouldExecuteBufferJump = true; // FIXME Тут проблема в том, что уже в FixedUpdate таймер успевает пройти
            _wasPressedThisFrame = false;
        }
    }


    public Vector2 UpdatePhysics(InputButtonState jumpState, Vector2 currentVelocity)
    {
        // HandleInput(jumpState);
        
        _moveVelocity = currentVelocity;

        if (_moveVelocity.y > 0f)
            _positiveMoveVelocity = true;

        if (!_collisionsChecker.IsGrounded && _jumpCoyoteTimer.IsFinished)
        {
            _numberAvailableJumps -= 1f;
            Debug.Log("jumpCoyoteTimerBUG");
        }
        
        
        bool isMultiJump = _wasPressedThisFrame && !_collisionsChecker.IsGrounded && !_jumpCoyoteTimer.IsRunning;
        
        if (_wasPressedThisFrame)
        {
            Debug.Log($"Jump attempt: Grounded={_collisionsChecker.IsGrounded}, " +
                      $"Coyote={_jumpCoyoteTimer.IsRunning}, " +
                      $"AvailableJumps={_numberAvailableJumps}, " +
                      $"CanMultiJump={isMultiJump}");
        }
        
        if (_wasPressedThisFrame && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning || _numberAvailableJumps > 0f))
        {
            Debug.Log("NormJump");
            
            ExecuteJump();

            _wasPressedThisFrame = false;

            _jumpCoyoteTimer.Stop();
            _jumpCoyoteTimer.Reset();

            _numberAvailableJumps -= 1;
        }

        // if (_jumpBufferTimer.IsRunning || _shouldExecuteBufferJump)
        if (_shouldExecuteBufferJump)
        {
            Debug.Log("BufferJump");

            ExecuteJump();

            if (!_isHeld)
            {
                ExecuteVariableJumpHeight();
            }

            _jumpBufferTimer.Stop();
            _jumpBufferTimer.Reset();

            _shouldExecuteBufferJump = false;
            _positiveMoveVelocity = false;

            _numberAvailableJumps -= 1;
        }

        if (_jumpKeyReleased)
        {
            ExecuteVariableJumpHeight();
            _jumpKeyReleased = false;
        }

        if (isMultiJump)
        {
            OnMultiJump?.Invoke();
        }

        // _moveVelocity = ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.JumpGravityMultiplayer);
        
        // _wasPressedThisFrame = false;
        // _jumpKeyReleased = false;
        // _shouldExecuteBufferJump = false;

        return _moveVelocity;
    }
    
    public Vector2 ApplyGravity(Vector2 moveVelocity, float gravity, float gravityMultiplayer)
    {
        // Применение гравитации
        moveVelocity.y -= gravity * gravityMultiplayer * Time.fixedDeltaTime;
        return moveVelocity;
    }

    public void OnExitJump()
    {
        _positiveMoveVelocity = false;
    }

    public bool CanMultiJump()
    {
        Debug.Log(_numberAvailableJumps > 0f);
        return _numberAvailableJumps > 0f;
    }

    public void ResetNumberAvailableJumps()
    {
        _numberAvailableJumps = _playerControllerStats.MaxNumberJumps;
        Debug.Log("_numberAvailableJumpsRESET");
    }

    public bool CanFall()
    {
        return (_moveVelocity.y < 0f && _positiveMoveVelocity);
    }

    // Метод для выполнения прыжка
    private void ExecuteJump()
    {
        // Изменения Y на высоту прыжка
        _moveVelocity.y = _playerControllerStats.MaxJumpVelocity;
        // _physicsContext.NumberAvailableJumps -= 1;
    }

    // Метод для выполнения неполного прыжка
    private void ExecuteVariableJumpHeight()
    {
        // Изменения Y на минимальную высоту прыжка
        if (_moveVelocity.y > _playerControllerStats.MinJumpVelocity)
        {
            _moveVelocity.y = _playerControllerStats.MinJumpVelocity;
        }
    }
    
    private void StartCoyoteTime()
    {
        // Тут я запускаю через событие койот таймер, для этого проверяю что он сошел с земли
        bool fallWithoutJump = _numberAvailableJumps == _playerControllerStats.MaxNumberJumps;
        if (fallWithoutJump)
        {
            // Debug.Log("COYOTESTART");
            _jumpCoyoteTimer.Start();
        }
    }
}