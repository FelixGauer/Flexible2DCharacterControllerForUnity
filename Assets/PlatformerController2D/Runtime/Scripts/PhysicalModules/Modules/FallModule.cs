using UnityEngine;

public class FallModule 
{
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CountdownTimer _jumpBufferTimer;
    private readonly CollisionsChecker _collisionsChecker;

    private bool _isHeld;

    public FallModule(PlayerControllerStats playerControllerStats, CountdownTimer jumpBufferTimer, CollisionsChecker collisionsChecker)
    {
        _playerControllerStats = playerControllerStats;
        _jumpBufferTimer = jumpBufferTimer;
        _collisionsChecker = collisionsChecker;
    }

    public void BufferJump(InputButtonState jumpState)
    {
        if (jumpState.WasPressedThisFrame) { _jumpBufferTimer.Start(); }
    }
    
    public void SetHoldState(bool isHeld) => _isHeld = isHeld;

    public Vector2 HandleFalling(Vector2 currentVelocity)
    {
        // if (_collisionsChecker.BumpedHead) currentVelocity = Vector2.zero; // FIXME
        
        float gravityMultiplier = GetGravityMultiplier(currentVelocity.y);
        
        Vector2 newVelocity = ApplyGravity(currentVelocity, _playerControllerStats.Gravity, gravityMultiplier);
        
        newVelocity.y = ClampFallSpeed(newVelocity.y);

        return newVelocity;
    }

    private Vector2 ApplyGravity(Vector2 moveVelocity, float gravity, float gravityMultiplayer)
    {
        moveVelocity.y -= gravity * gravityMultiplayer * Time.fixedDeltaTime;
        return moveVelocity;
    }
    
    private float GetGravityMultiplier(float verticalVelocity)
    {
        // if (_collisionsChecker.BumpedHead) // FIXME
        // {
        //     return _playerControllerStats.FallGravityMultiplayer;
        // }
        if (Mathf.Abs(verticalVelocity) < _playerControllerStats.JumpHangTimeThreshold)
        {
            return _playerControllerStats.JumpHangGravityMultiplier;
        }
        else if (_isHeld)
        {
            return _playerControllerStats.JumpGravityMultiplayer;
        }
        else
        {
            return _playerControllerStats.FallGravityMultiplayer;
        }
    }
    
    private float ClampFallSpeed(float verticalVelocity)
    {
        return Mathf.Clamp(verticalVelocity, 
            -_playerControllerStats.MaxFallSpeed, 
            _playerControllerStats.MaxUpwardSpeed);
    }
}