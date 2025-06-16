using UnityEngine;

public class CrouchRollModule
{
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CountdownTimer _crouchRollTimer;
    private readonly TurnChecker _turnChecker;
	
    private bool IsFacingRight => _turnChecker.IsFacingRight;

    private Vector2 _crouchRollDirection;

    public CrouchRollModule(PlayerControllerStats playerControllerStats, TurnChecker turnChecker, CountdownTimer crouchRollTimer) 
    {
        _playerControllerStats = playerControllerStats;
        _turnChecker = turnChecker;
        _crouchRollTimer = crouchRollTimer;
    }
	
    public Vector2 CrouchRoll(Vector2 moveVelocity)
    {
        moveVelocity.x = _crouchRollDirection.x * _playerControllerStats.CrouchRollVelocity;
        return moveVelocity;
    }
	
    public void StartCrouchRoll(Vector2? customDirection = null)
    {
        _crouchRollTimer.Start();
        _crouchRollDirection = customDirection ?? (IsFacingRight ? Vector2.right : Vector2.left);
    }
	
    public void StopCrouchRoll()
    {
        _crouchRollTimer.Stop();
        _crouchRollTimer.Reset();
    }
    
    // public bool IsCrouchRollTimerRunning() => _crouchRollTimer.IsRunning;
    // public bool IsCrouchRollTimerFinished() => _crouchRollTimer.IsFinished;
    // public void UpdateTimer() => _crouchRollTimer.Tick(Time.deltaTime);
}