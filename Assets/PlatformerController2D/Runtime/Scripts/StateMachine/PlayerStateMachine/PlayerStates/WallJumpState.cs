using UnityEngine;
using UnityEngine.PlayerLoop;

public class WallJumpState : BaseState
{
    private readonly WallJumpModule _wallJumpModule;
    private readonly WallSlideModule _wallSlideModule;
    
    private Vector2 _moveVelocity;

    public WallJumpState(PlayerStateContext context, WallJumpModule wallJumpModule, WallSlideModule wallSlideModule) : base(context)
    {
        _wallJumpModule = wallJumpModule;
        _wallSlideModule = wallSlideModule;
    }

    public override void OnEnter()
    {
        Debug.Log("WallJumpState");
        
        _wallJumpModule.StartWallJump();
    }

    public override void FixedUpdate()
    {
        var wallDirectionX = _wallSlideModule.CurrentWallDirection;
        _moveVelocity = _wallJumpModule.HandleWallJump(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), wallDirectionX);
        physicsHandler2D.AddVelocity(_moveVelocity);
    }

    public override void OnExit()
    {
        animationController.PlayAnimation("Jump");

        inputReader.ResetFrameStates();
    }
}