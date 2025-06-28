using UnityEngine;

public class PlayerTimerRegistry
{
    public CountdownTimer jumpCoyoteTimer { get; private set; }
    public CountdownTimer jumpBufferTimer { get; private set; }
    public CountdownTimer wallFallTimer { get; private set; }
    public CountdownTimer dashTimer { get; private set; }
    public CountdownTimer crouchRollTimer { get; private set; }
    
    public CountdownTimer wallJumpTimer { get; private set; }
 

    public PlayerTimerRegistry(PlayerControllerStats playerControllerStats)
    {
        jumpCoyoteTimer = new CountdownTimer(() => playerControllerStats.CoyoteTime);
        jumpBufferTimer = new CountdownTimer(() => playerControllerStats.BufferTime);
        wallFallTimer = new CountdownTimer(() => playerControllerStats.WallFallTime);
        dashTimer = new CountdownTimer(() => playerControllerStats.DashTime);
        crouchRollTimer = new CountdownTimer(() => playerControllerStats.CrouchRollTime);
        
        wallJumpTimer = new CountdownTimer(() => playerControllerStats.WallJumpTime);
    }
	
    public void UpdateAll() // Сделать список
    {
        jumpCoyoteTimer.Tick(Time.deltaTime);
        jumpBufferTimer.Tick(Time.deltaTime);
        wallFallTimer.Tick(Time.deltaTime);
        dashTimer.Tick(Time.deltaTime);
        crouchRollTimer.Tick(Time.deltaTime);
        
        wallJumpTimer.Tick(Time.deltaTime);
    }
}