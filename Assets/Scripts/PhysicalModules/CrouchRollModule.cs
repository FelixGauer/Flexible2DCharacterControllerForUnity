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

    public CrouchRollModule(PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, CountdownTimer crouchRollTimer, TurnChecker turnChecker) 
    {
        _physicsContext = physicsContext;
        _playerControllerStats = playerControllerStats;
        _crouchRollTimer = crouchRollTimer;
        _turnChecker = turnChecker;
    }
	
    public Vector2 CrouchRoll(Vector2 moveVelocity)
    {
        // moveVelocity = _physicsContext.MoveVelocity;
        moveVelocity.x = _crouchRollDirection.x * _playerControllerStats.CrouchRollVelocity;
        // _physicsContext.MoveVelocity = moveVelocity;
        return moveVelocity;
    }
	
    // Метод вызываемый при входе в состояние кувырка в приседе
    public void StartCrouchRoll()
    {
        _crouchRollTimer.Start();
        _crouchRollDirection = IsFacingRight ? Vector2.right : Vector2.left;
    }
	
    // Метод вызываемый при выходе из кувырка в приседе
    public void StopCrouchRoll()
    {
        _crouchRollTimer.Stop();
        _crouchRollTimer.Reset();
    }
}