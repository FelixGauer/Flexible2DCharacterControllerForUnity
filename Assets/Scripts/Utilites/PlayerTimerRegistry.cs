using UnityEngine;

public class PlayerTimerRegistry
{
    public CountdownTimer jumpCoyoteTimer { get; private set; }
    public CountdownTimer jumpBufferTimer { get; private set; }
    public CountdownTimer wallJumpTimer { get; private set; }
    public CountdownTimer dashTimer { get; private set; }
    public CountdownTimer crouchRollTimer { get; private set; }

    public PlayerTimerRegistry(PlayerControllerStats playerControllerStats)
    {
        jumpCoyoteTimer = new CountdownTimer(() => playerControllerStats.CoyoteTime);
        jumpBufferTimer = new CountdownTimer(() => playerControllerStats.BufferTime);
        wallJumpTimer = new CountdownTimer(() => playerControllerStats.WallJumpTime);
        dashTimer = new CountdownTimer(() => playerControllerStats.DashTime);
        crouchRollTimer = new CountdownTimer(() => playerControllerStats.CrouchRollTime);
    }
	
    public void UpdateAll() // Сделать список
    {
        jumpCoyoteTimer.Tick(Time.deltaTime);
        jumpBufferTimer.Tick(Time.deltaTime);
        wallJumpTimer.Tick(Time.deltaTime);
        dashTimer.Tick(Time.deltaTime);
        crouchRollTimer.Tick(Time.deltaTime);
    }
}