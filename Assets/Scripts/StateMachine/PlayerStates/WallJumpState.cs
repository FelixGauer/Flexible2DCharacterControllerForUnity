using UnityEngine;

public class WallJumpState : BaseState
{
    public WallJumpState(PlayerController player) : base(player) { }

    public override void OnEnter()
    {
        Debug.Log("WallSlideState");
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