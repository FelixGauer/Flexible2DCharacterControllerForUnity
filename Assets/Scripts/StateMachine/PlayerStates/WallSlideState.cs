using UnityEngine;


public class WallSlideState : BaseState
{
	public WallSlideState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		Debug.Log("WallJumpState");

		// player.OnEnterWallSliding();	
		
		player.playerPhysicsController.WallSlideModule.OnEnterWallSliding();

	}

	public override void FixedUpdate()
	{
		// player.HandleWallInteraction();
		
		// player.playerPhysicsController.WallSlideModule.HandleWallInteraction(player.GetMoveDirection(), player.input.JumpInputButtonState);
		
		player.playerPhysicsController.WallSlideModule.HandleWallSlide(player.GetMoveDirection());
	}

    public override void OnExit()
    {
        // player.OnExitWallSliding();
        
        player.playerPhysicsController.WallSlideModule.OnExitWallSliding();

    }
}

public class WallJumpState : BaseState
{
	public WallJumpState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		Debug.Log("WallSlideState");
		player.playerPhysicsController.WallSlideModule.HandleWallJump(player.GetMoveDirection());

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

