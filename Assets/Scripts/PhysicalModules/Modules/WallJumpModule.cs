using UnityEngine;

public class WallJumpModule
{
    private readonly PlayerControllerStats _playerControllerStats;
	
    private Vector2 _moveVelocity;

    public WallJumpModule(PlayerControllerStats playerControllerStats) 
    {
        _playerControllerStats = playerControllerStats;
        
    }
    
    public Vector2 HandleWallJump(Vector2 moveVelocity, Vector2 moveDirection, float wallDirectionX)
    {
        // Расчет направление персонажа по X
        // float wallDirectionX = CalculateWallDirectionX();

        if (moveDirection.x == wallDirectionX) // Если ввод в сторону стены
        {
            // Прыжок вверх по стене
            moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpClimb.x, _playerControllerStats.WallJumpClimb.y);
        }
        else if (moveDirection.x == 0f) // Если ввод равен 0
        {
            // Прыжок от стены
            moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpOff.x, _playerControllerStats.WallJumpOff.y);
        }
        else // Если ввод сторону от стены, обратную сторону
        {
            // Прыжок в сторону от стены
            moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallLeap.x, _playerControllerStats.WallLeap.y);
        }
		
        return moveVelocity;
    }
}