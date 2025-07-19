using UnityEngine;

public class FallModule 
{
    public FallModule(PlayerControllerStats playerControllerStats, CountdownTimer jumpBufferTimer)
    {
        _playerControllerStats = playerControllerStats;
        _jumpBufferTimer = jumpBufferTimer;
    }
    
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CountdownTimer _jumpBufferTimer;

    private bool _isHeld;

    public void BufferJump(InputButtonState jumpState)
    {
        if (jumpState.WasPressedThisFrame) { _jumpBufferTimer.Start(); }
    }
    
    public void SetHoldState(bool isHeld) => _isHeld = isHeld;
    
    public Vector2 HandleFalling(Vector2 currentVelocity)
    {
        float gravityMultiplier = GetGravityMultiplier(currentVelocity.y);
        
        // Применяем гравитацию
        Vector2 newVelocity = ApplyGravity(currentVelocity, _playerControllerStats.Gravity, gravityMultiplier);
        
        // Ограничиваем максимальную скорость падения
        newVelocity.y = ClampFallSpeed(newVelocity.y);

        return newVelocity;
    }

    private Vector2 ApplyGravity(Vector2 moveVelocity, float gravity, float gravityMultiplayer)
    {
        // Применение гравитации
        moveVelocity.y -= gravity * gravityMultiplayer * Time.fixedDeltaTime;
        return moveVelocity;
    }
    
    private float GetGravityMultiplier(float verticalVelocity)
    {
        // Если «висим» на верхней точке прыжка (ниже порога hang-time)
        if (Mathf.Abs(verticalVelocity) < _playerControllerStats.JumpHangTimeThreshold)
        {
            return _playerControllerStats.JumpHangGravityMultiplier;
        }
        // Если ещё держат кнопку прыжка — уменьшаем силу гравитации
        else if (_isHeld)
        {
            return _playerControllerStats.JumpGravityMultiplayer;
        }
        // Свободное падение или отпустили кнопку
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