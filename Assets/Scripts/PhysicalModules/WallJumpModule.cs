using UnityEngine;

public class WallJumpModule
{
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly TurnChecker _turnChecker;
	
    private Vector2 _moveVelocity;

    private bool IsFacingRight => _turnChecker.IsFacingRight;

    public WallJumpModule(PlayerControllerStats playerControllerStats, TurnChecker turnChecker) 
    {
        _playerControllerStats = playerControllerStats;
        _turnChecker = turnChecker;
    }
    
    public Vector2 HandleWallJump(Vector2 _moveVelocity, Vector2 moveDirection)
    {
        // _moveVelocity = _physicsContext.MoveVelocity;
        
        // Расчет направление персонажа по X
        float wallDirectionX = CalculateWallDirectionX();

        if (moveDirection.x == wallDirectionX) // Если ввод в сторону стены
        {
            // Прыжок вверх по стене
            _moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpClimb.x, _playerControllerStats.WallJumpClimb.y);
        }
        else if (moveDirection.x == 0f) // Если ввод равен 0
        {
            // Прыжок от стены
            _moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpOff.x, _playerControllerStats.WallJumpOff.y);
        }
        else // Если ввод сторону от стены, обратную сторону
        {
            // Прыжок в сторону от стены
            _moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallLeap.x, _playerControllerStats.WallLeap.y);
        }
		
        // _physicsContext.MoveVelocity = _moveVelocity;
        
        // _physicsHandler2D.AddVelocity(_moveVelocity);
        
        return _moveVelocity;
    }
    
    // Метод вычисляет направление стены по X
    public float CalculateWallDirectionX()
    {
        return IsFacingRight ? 1f : -1f;
    }
}