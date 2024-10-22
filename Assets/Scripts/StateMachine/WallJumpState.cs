using UnityEngine;


public class WallJumpState : BaseState
{
	public WallJumpState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		// player.GroundedState();		
		Debug.Log("WallJumpState");
	}

	public override void FixedUpdate()
	{
		player.HandleWallSlide();
		// player.HandleMovement();
	}
}
