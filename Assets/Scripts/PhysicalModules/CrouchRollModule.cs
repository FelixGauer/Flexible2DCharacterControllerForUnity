using UnityEngine;

public class CrouchRollModule
{
    private readonly PhysicsContext _physicsContext;
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CountdownTimer _crouchRollTimer;
    private readonly TurnChecker _turnChecker;
	
    private bool IsFacingRight => _turnChecker.IsFacingRight;

    // private Vector2 _moveVelocity;
    private Vector2 _crouchRollDirection;

    public CrouchRollModule(PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, TurnChecker turnChecker, CountdownTimer crouchRollTimer) 
    {
        _physicsContext = physicsContext;
        _playerControllerStats = playerControllerStats;
        _turnChecker = turnChecker;
        _crouchRollTimer = crouchRollTimer;

        // _crouchRollTimer = new CountdownTimer(playerControllerStats.CrouchRollTime);
    }
	
    public Vector2 CrouchRoll(Vector2 moveVelocity)
    {
        moveVelocity.x = _crouchRollDirection.x * _playerControllerStats.CrouchRollVelocity;
        return moveVelocity;
    }
	
    public void StartCrouchRoll()
    {
        _crouchRollTimer.Start();
        _crouchRollDirection = IsFacingRight ? Vector2.right : Vector2.left;
    }
	
    public void StopCrouchRoll()
    {
        _crouchRollTimer.Stop();
        _crouchRollTimer.Reset();
    }
    //
    // public bool IsCrouchRollTimerRunning() => _crouchRollTimer.IsRunning;
    // public bool IsCrouchRollTimerFinished() => _crouchRollTimer.IsFinished;
    // public void UpdateTimer() => _crouchRollTimer.Tick(Time.deltaTime);
}