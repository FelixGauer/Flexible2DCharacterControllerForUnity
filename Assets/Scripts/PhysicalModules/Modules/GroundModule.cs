using UnityEngine;

public class GroundModule
{
    private readonly PlayerControllerStats _playerControllerStats;
	
    private Vector2 _moveVelocity;

    private readonly JumpModule _jumpModule;
    private readonly DashModule _dashModule;

    public GroundModule(PlayerControllerStats playerControllerStats, JumpModule jumpModule, DashModule dashModule) 
    {
        _jumpModule = jumpModule;
        _playerControllerStats = playerControllerStats;
        _dashModule = dashModule;
    }

    public void HandleGround()
    {
        // _physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
        // _physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash; // При касании земли возвращение рывков
        
        // _jumpModule.ResetNumberAvailableJumps();
        // _dashModule.ResetNumberAvailableDash();

        // _moveVelocity.y = _playerControllerStats.GroundGravity; // Гравитация на земле 
        
        // _physicsHandler2D.AddVelocity(_moveVelocity);
    }
}