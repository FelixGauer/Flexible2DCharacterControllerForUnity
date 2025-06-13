using UnityEngine;

public class PhysicsContext 
{
    public Vector2 MoveVelocity { get; set; }
    // public bool VariableJumpHeight { get; set; }
    // public bool WasWallSliding {get; set;}

    public float NumberAvailableJumps { get; set; }
    public float NumberAvailableDash { get; set; }
	
    public Vector2 ApplyGravity(Vector2 moveVelocity, float gravity, float gravityMultiplayer)
    {
        // Применение гравитации
        moveVelocity.y -= gravity * gravityMultiplayer * Time.fixedDeltaTime;
        return moveVelocity;
    }
    
	
    // public void CoyoteTimerStart()
    // {
    // 	_jumpCoyoteTimer.Start();
    // }
	
    // public void HandleGround()
    // {
    // 	NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
    // 	NumberAvailableDash = _playerControllerStats.MaxNumberDash; // При касании земли возвращение рывков
    //
    // 	MoveVelocity = _playerControllerStats.GroundGravity; // Гравитация на земле 
    // }
}