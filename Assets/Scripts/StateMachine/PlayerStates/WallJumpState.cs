using UnityEngine;

public class WallJumpState : BaseState
{
    public WallJumpState(PlayerController player, Animator animator) : base(player, animator) { }

    public override void OnEnter()
    {
        Debug.Log("WallJumpState");
        // player.playerPhysicsController.WallSlideModule.HandleWallJump(player.GetMoveDirection());
        player.playerPhysicsController.WallJumpModule.HandleWallJump(player.GetMoveDirection());
    }

    public override void FixedUpdate()
    {
        // player.playerPhysicsController.WallSlideModule.HandleWallJump(player.GetMoveDirection());
    }

    public override void OnExit()
    {
        player.input.DashInputButtonState.ResetFrameState();
        player.input.JumpInputButtonState.ResetFrameState();
		
        // player.playerPhysicsController.WallSlideModule.OnExitWallSliding();
    }
}