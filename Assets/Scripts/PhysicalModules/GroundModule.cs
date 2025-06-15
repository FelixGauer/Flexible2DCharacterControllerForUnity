using UnityEngine;

public class GroundModule
{
    private readonly Rigidbody2D _rigidbody;

    private readonly PlayerControllerStats _playerControllerStats;
    private readonly PhysicsContext _physicsContext;
	
    private Vector2 _moveVelocity;

    private readonly PhysicsHandler2D _physicsHandler2D;
    private readonly JumpModule _jumpModule;
    private readonly DashModule _dashModule;

    public GroundModule(PlayerControllerStats playerControllerStats , PhysicsContext physicsContext, PhysicsHandler2D physicsHandler2D, JumpModule jumpModule, DashModule dashModule) 
    {
        _playerControllerStats = playerControllerStats;
        _physicsContext = physicsContext;
        _physicsHandler2D = physicsHandler2D;
        _jumpModule = jumpModule;
        _dashModule = dashModule;
    }

    public void HandleGround()
    {
        // _physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
        // _physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash; // При касании земли возвращение рывков
        
        _jumpModule.ResetNumberAvailableJumps();
        _dashModule.ResetNumberAvailableDash();

        _moveVelocity.y = _playerControllerStats.GroundGravity; // Гравитация на земле 
        
        _physicsHandler2D.AddVelocity(_moveVelocity);
    }
}