using UnityEngine;

public class GroundModule
{
    private readonly Rigidbody2D _rigidbody;

    private readonly PlayerControllerStats _playerControllerStats;
    private readonly PhysicsContext _physicsContext;
	
    private Vector2 _moveVelocity;

    public GroundModule(PlayerControllerStats playerControllerStats , PhysicsContext physicsContext) 
    {
        _playerControllerStats = playerControllerStats;
        _physicsContext = physicsContext;
    }

    public void HandleGround()
    {
        _physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
        _physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash; // При касании земли возвращение рывков

        _moveVelocity.y = _playerControllerStats.GroundGravity; // Гравитация на земле 

        _physicsContext.MoveVelocity = _moveVelocity;
    }
}