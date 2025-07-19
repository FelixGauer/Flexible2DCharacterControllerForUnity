using UnityEngine;

public class WallJumpModule
{
    private readonly PlayerControllerStats _playerControllerStats;
    private readonly CountdownTimer _timer;

    private Vector2 _moveVelocity;

    public WallJumpModule(PlayerControllerStats playerControllerStats, CountdownTimer timer)
    {
        _playerControllerStats = playerControllerStats;
        _timer = timer;
    }

    public void StartWallJump()
    {
        _timer.Start();
    }
    
    public Vector2 HandleWallJump(Vector2 moveVelocity, Vector2 moveDirection, float wallDirectionX)
    {
        float inputSign = Mathf.Sign(moveDirection.x);
        float wallSign = Mathf.Sign(wallDirectionX);
    
        if (inputSign == wallSign && !Mathf.Approximately(moveDirection.x, 0f)) // Ввод в сторону стены
        {
            // Прыжок вверх по стене
            moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpClimb.x, _playerControllerStats.WallJumpClimb.y);
        }
        else if (Mathf.Approximately(moveDirection.x, 0f) && _timer.IsFinished) // Ввод равен 0
        {
            // Прыжок от стены
            moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpOff.x, _playerControllerStats.WallJumpOff.y);
        }
        else if (inputSign == -wallSign && !Mathf.Approximately(moveDirection.x, 0f)) // Ввод в сторону от стены
        {
            // Прыжок в сторону от стены
            moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallLeap.x, _playerControllerStats.WallLeap.y);
        }

        return moveVelocity;
    }
}