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
        
        _collisionsChecker.OnWallLeft += () => ResetNumberAvailableJump(isWall: true);
        _collisionsChecker.OnWallLeft += ResetJumpCoyoteTimer;
        
        _collisionsChecker.OnGroundTouched += () => ResetNumberAvailableJump(isWall: false);

        _collisionsChecker.OnGroundTouched += ResetJumpCoyoteTimer;
        
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
    }

    public Vector2 JumpPhysicsProcessing(Vector2 currentVelocity)
    {
        var moveVelocity = currentVelocity;
        
        if (moveVelocity.y > 0f)
            _positiveMoveVelocity = true;
        
        bool jump = _jumpInputPressed && _numberAvailableJumps > 0f;
        bool multiJump = jump && !fullSetOfJumps;
        bool variableJump = _jumpKeyReleased;
        bool coyoteJump = _jumpCoyoteTimer.IsRunning;
        bool bufferJump = _shouldExecuteBufferJump;
        bool bufferVariableJump = bufferJump && !_isHeld;

        if (jump || bufferJump || coyoteJump) // FIXME
        {
            moveVelocity = PerformJump(currentVelocity);

            _numberAvailableJumps -= 1;

            _shouldExecuteBufferJump = false;

            _positiveMoveVelocity = false;
            
            ResetJumpCoyoteTimer();
        }

        if (variableJump)
        {
            moveVelocity = PerformVariableJumpHeight(moveVelocity);
            _jumpKeyReleased = false;
        }
        
        if (bufferVariableJump)
        {
            moveVelocity = PerformVariableJumpHeight(moveVelocity);
            _shouldExecuteBufferJump = false;
        }
        
        if (multiJump) OnMultiJump?.Invoke();
        
        _jumpInputPressed = false;
        
        return moveVelocity;
    }

    public void OnExitJump()
    {
        _positiveMoveVelocity = false;
    }

    public bool CanFall(Vector2 currentVelocity)
    {
        return (currentVelocity.y < 0f && _positiveMoveVelocity && !_jumpInputPressed); // < 
    }
    
    public bool InFlight()
    {
        return (_positiveMoveVelocity);
    }

    public bool CanMultiJump()
    {
        return _numberAvailableJumps > 0f;
    }

    private Vector2 PerformJump(Vector2 currentVelocity)
    {
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
}