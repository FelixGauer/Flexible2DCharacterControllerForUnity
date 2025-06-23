using UnityEngine;

public class WallJumpState : BaseState
{
    private Vector2 _moveVelocity;

    public WallJumpState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
        base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

    public override void OnEnter()
    {
        Debug.Log("WallJumpState");
        
        animationController.PlayAnimation("Jump");

        var wallDirectionX = playerPhysicsController.WallSlideModule.CurrentWallDirection;
        _moveVelocity = playerPhysicsController.WallJumpModule.HandleWallJump(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), wallDirectionX);
        physicsHandler2D.AddVelocity(_moveVelocity);
    }
    
    public override void OnExit()
    {
        inputReader.ResetFrameStates();
    }
}