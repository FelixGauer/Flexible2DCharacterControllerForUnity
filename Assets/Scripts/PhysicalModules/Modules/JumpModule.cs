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
        
        
        // _collisionsChecker.OnWallLeft += ResetNumberAvailableJumps;
        // _collisionsChecker.OnWallLeft += ResetNumberAvailableJumpsOnWall;
        _collisionsChecker.OnWallLeft += () => ResetNumberAvailableJump(isWall: true);
        _collisionsChecker.OnWallLeft += ResetJumpCoyoteTimer;
        
        // _collisionsChecker.OnGroundTouched += ResetNumberAvailableJumps;
        _collisionsChecker.OnGroundTouched += () => ResetNumberAvailableJump(isWall: false);

        _collisionsChecker.OnGroundTouched += ResetJumpCoyoteTimer;
        
        
        // _collisionsChecker.OnWallTouched += () => {
        //     if (CanFall(_moveVelocity)) ResetNumberAvailableJumps();
        // };
        // _collisionsChecker.OnWallTouched += () => {
        //     if (CanFall(_moveVelocity)) ResetJumpCoyoteTimer();
        // };
        //
        // _collisionsChecker.OnGroundLeft += StartCoyoteTime;

        // _jumpCoyoteTimer.OnTimerFinished += CoyoteJumpDeleteJump;
        
        _collisionsChecker.OnGroundLeft += () => {
            if (fullSetOfJumps) _jumpCoyoteTimer.Start();
        };
        
        _jumpCoyoteTimer.OnTimerFinished += () => {
            if (fullSetOfJumps) _numberAvailableJumps--;
        };
    }

    public event System.Action OnMultiJump;

    private readonly CollisionsChecker _collisionsChecker;
    private readonly PlayerControllerStats _playerControllerStats;

    private readonly CountdownTimer _jumpCoyoteTimer;
    private readonly CountdownTimer _jumpBufferTimer;

    private Vector2 _moveVelocity;
    private bool _positiveMoveVelocity;
    private float _numberAvailableJumps;

    private InputButtonState _jumpState;

    private bool _jumpKeyReleased;
    private bool _jumpInputPressed;
    private bool _isHeld;
    private bool _shouldExecuteBufferJump;

    public void TestStartJump()
    {
        _jumpInputPressed = true;
    }

    public void HandleInput(InputButtonState jumpState)
    {
        if (jumpState.WasPressedThisFrame) 
            _jumpInputPressed = true;
        
        if (jumpState.WasReleasedThisFrame) 
            _jumpKeyReleased = true;
        
        _isHeld = jumpState.IsHeld;
        
        if (_jumpBufferTimer.IsRunning)
        {
            _shouldExecuteBufferJump = true;
            
            _jumpBufferTimer.Stop();
            _jumpBufferTimer.Reset();
            
            _jumpInputPressed = false;
        } 
        
        // TrackJumpHeight();
    }

    public Vector2 JumpPhysicsProcessing(Vector2 currentVelocity)
    {
        var moveVelocity = currentVelocity;
        
        if (moveVelocity.y > 0f)
            _positiveMoveVelocity = true;
        
        // if (_jumpCoyoteTimer.IsFinished)
        //     _numberAvailableJumps -= 1f;
        
        bool jump = _jumpInputPressed && _numberAvailableJumps > 0f;
        bool multiJump = jump && !fullSetOfJumps;
        bool variableJump = _jumpKeyReleased;
        bool coyoteJump = _jumpCoyoteTimer.IsRunning;
        bool bufferJump = _shouldExecuteBufferJump;
        bool bufferVariableJump = bufferJump && !_isHeld;

        if (jump || bufferJump || coyoteJump) // FIXME
        {
            Debug.Log(_numberAvailableJumps);
            moveVelocity = PerformJump(currentVelocity);

            _numberAvailableJumps -= 1;

            _shouldExecuteBufferJump = false;
            
            ResetJumpCoyoteTimer();
        }

        if (variableJump)
        {
            moveVelocity = PerformVariableJumpHeight(moveVelocity);
            _jumpKeyReleased = false;
        }
        
        if (bufferVariableJump)
        {
            Debug.Log("VAR");
            moveVelocity = PerformVariableJumpHeight(moveVelocity);
            _shouldExecuteBufferJump = false;
        }
        
        if (multiJump) OnMultiJump?.Invoke();
        
        _jumpInputPressed = false;
        
        return moveVelocity;
    }

    public void OnExitJump()
    {
        // FinishJumpHeightTracking();
        _positiveMoveVelocity = false;
    }
    
    public bool CanFall(Vector2 currentVelocity)
    {
        return (currentVelocity.y < 0f && _positiveMoveVelocity && !_jumpInputPressed);
    }

    public bool CanMultiJump()
    {
        return _numberAvailableJumps > 0f;
    }

    private Vector2 PerformJump(Vector2 currentVelocity)
    {
        // StartJumpHeightTracking(); // Начинаем отслеживание высоты

        currentVelocity.y = _playerControllerStats.MaxJumpVelocity;
        return currentVelocity;
    }

    private Vector2 PerformVariableJumpHeight(Vector2 currentVelocity)
    {
        if (currentVelocity.y > _playerControllerStats.MinJumpVelocity)
        {
            currentVelocity.y = _playerControllerStats.MinJumpVelocity;
        }

        return currentVelocity;
    }

    private bool fullSetOfJumps => _numberAvailableJumps >= _playerControllerStats.MaxNumberJumps;
    
    private void ResetJumpCoyoteTimer()
    {
        _jumpCoyoteTimer.Stop();
        _jumpCoyoteTimer.Reset();
    }
    
    // private void ResetNumberAvailableJumps()
    // {
    //     _numberAvailableJumps = _playerControllerStats.MaxNumberJumps;
    // }
    //
    // private void ResetNumberAvailableJumpsOnWall()
    // {
    //     if (_playerControllerStats.WallJumpRecovery)
    //         _numberAvailableJumps = _playerControllerStats.MaxNumberJumps;
    //     else
    //         _numberAvailableJumps = 0f;
    // }
    
    private void ResetNumberAvailableJump(bool isWall)
    {
        if (isWall && !_playerControllerStats.ResetNumberJumpOnWall)
        {
            _numberAvailableJumps = 0f;
        }
        else
        {
            _numberAvailableJumps = _playerControllerStats.MaxNumberJumps;
        }
    }
    

    

    
    

    
    
    
    
    
    
    //
    //
    // public void StartJump(InputButtonState jumpState)
    // {
    //     _jumpInputPressed = true;
    //     _positiveMoveVelocity = false;
    // }
    //
    // public void HandleInput(InputButtonState jumpState)
    // {
    //     if (jumpState.WasPressedThisFrame)
    //     {
    //         CreateJumpVisual();
    //     }
    //     
    //     if (jumpState.WasPressedThisFrame) _jumpInputPressed = true;
    //     if (jumpState.WasReleasedThisFrame) _jumpKeyReleased = true;
    //     _isHeld = jumpState.IsHeld;
    //
    //     if (_jumpBufferTimer.IsRunning)
    //     {
    //         _shouldExecuteBufferJump = true;
    //         _jumpInputPressed = false;
    //     }
    // }
    //
    // public Vector2 UpdatePhysics(InputButtonState jumpState, Vector2 currentVelocity)
    // {
    //     _moveVelocity = currentVelocity;
    //
    //     if (_moveVelocity.y > 0f)
    //         _positiveMoveVelocity = true;
    //
    //     // Отслеживание высоты прыжка
    //     StartJumpHeightTracking(); // Начинаем отслеживание высоты
    //
    //
    //     if (!_collisionsChecker.IsGrounded && _jumpCoyoteTimer.IsFinished)
    //     {
    //         _numberAvailableJumps -= 1f;
    //     }
    //     
    //     bool isMultiJump = _jumpInputPressed && !_collisionsChecker.IsGrounded && !_jumpCoyoteTimer.IsRunning && _numberAvailableJumps > 0f;
    //     
    //     if (_jumpInputPressed && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning || _numberAvailableJumps > 0f))
    //     {
    //         TrackJumpHeight();
    //
    //         ExecuteJump();
    //
    //         _jumpInputPressed = false;
    //
    //         _jumpCoyoteTimer.Stop();
    //         _jumpCoyoteTimer.Reset();
    //
    //         _numberAvailableJumps -= 1;
    //         
    //         _positiveMoveVelocity = false;
    //     }
    //
    //     if (_shouldExecuteBufferJump)
    //     {
    //         ExecuteJump();
    //         StartJumpHeightTracking(); // Начинаем отслеживание высоты
    //
    //         if (!_isHeld)
    //         {
    //             ExecuteVariableJumpHeight();
    //         }
    //
    //         _jumpBufferTimer.Stop();
    //         _jumpBufferTimer.Reset();
    //
    //         _shouldExecuteBufferJump = false;
    //         _positiveMoveVelocity = false;
    //
    //         _numberAvailableJumps -= 1;
    //     }
    //
    //     if (_jumpKeyReleased)
    //     {
    //         ExecuteVariableJumpHeight();
    //         _jumpKeyReleased = false;
    //     }
    //
    //     if (isMultiJump)
    //     {
    //         OnMultiJump?.Invoke();
    //     }
    //
    //     return _moveVelocity;
    // }
    //
    // public void OnExitJump()
    // {
    //     _positiveMoveVelocity = false;
    // }
    //
    //
    //
    // public void ResetNumberAvailableJumps()
    // {
    //     _numberAvailableJumps = _playerControllerStats.MaxNumberJumps;
    //     
    //     // Завершаем отслеживание прыжка при приземлении
    //     // if (_isTrackingJump)
    //     // {
    //     //     FinishJumpHeightTracking();
    //     // }
    // }
    //
    // public bool CanFall()
    // {
    //     return (_moveVelocity.y < 0f && _positiveMoveVelocity && !_jumpInputPressed);
    // }
    //
    // private void ExecuteJump()
    // {
    //     _moveVelocity.y = _playerControllerStats.MaxJumpVelocity;
    // }
    //
    // private void ExecuteVariableJumpHeight()
    // {
    //     if (_moveVelocity.y > _playerControllerStats.MinJumpVelocity)
    //     {
    //         _moveVelocity.y = _playerControllerStats.MinJumpVelocity;
    //     }
    // }
    //
    // private void StartCoyoteTime()
    // {
    //     // bool fallWithoutJump = _numberAvailableJumps >= _playerControllerStats.MaxNumberJumps;
    //     if (fullSetOfJumps)
    //     {
    //         _jumpCoyoteTimer.Start();
    //     }
    // }
    //
    // private void CoyoteJumpDeleteJump()
    // {
    //     if (fullSetOfJumps)
    //     {
    //         _numberAvailableJumps -= 1f;
    //     }
    // }

    
    
    // Ссылка на игрока для получения позиции
    private Transform _playerTransform;
    
    // Переменные для отслеживания высоты прыжка
    private bool _isTrackingJump = false; // Отслеживаем ли сейчас прыжок
    private float _jumpStartY; // Y координата начала прыжка
    private float _maxJumpHeight; // Максимальная высота текущего прыжка
    private int _jumpCounter = 0; // Счетчик прыжков для логов
    
    // Метод для установки ссылки на игрока
    public void SetPlayerTransform(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    // Создание визуального эффекта в месте прыжка
    private void CreateJumpVisual()
    {
        if (_playerTransform == null) return;

        Vector3 jumpPosition = _playerTransform.position;
        JumpVisualEffect.CreateJumpMark(jumpPosition);
    }
    
    // Начинаем отслеживание высоты прыжка
    private void StartJumpHeightTracking()
    {
        if (_playerTransform == null) return;
        
        _isTrackingJump = true;
        _jumpStartY = _playerTransform.position.y;
        _maxJumpHeight = 0f;
        _jumpCounter++;
        
        Debug.Log($"[JUMP #{_jumpCounter}] Начало отслеживания прыжка. Стартовая высота: {_jumpStartY:F2}");
    }
    
    // Отслеживаем максимальную высоту во время прыжка
    private void TrackJumpHeight()
    {
        if (!_isTrackingJump || _playerTransform == null) return;
        
        float currentHeight = _playerTransform.position.y - _jumpStartY;
        
        // Обновляем максимальную высоту если текущая больше
        if (currentHeight > _maxJumpHeight)
        {
            _maxJumpHeight = currentHeight;
        }
        
        // Если игрок начал падать (скорость отрицательная) и мы еще отслеживаем
        // это означает что мы достигли пика прыжка
        if (_moveVelocity.y <= 0f && _maxJumpHeight > 0.1f)
        {
            // Debug.Log($"[JUMP #{_jumpCounter}] Пик прыжка достигнут! Максимальная высота: {_maxJumpHeight:F2} units");
        }
    }
    
    // Завершаем отслеживание прыжка (при приземлении)
    private void FinishJumpHeightTracking()
    {
        if (!_isTrackingJump) return;
        
        Debug.Log($"[JUMP #{_jumpCounter}] Приземление! Итоговая максимальная высота прыжка: {_maxJumpHeight:F2} units");
        
        _isTrackingJump = false;
        _maxJumpHeight = 0f;
    }
}