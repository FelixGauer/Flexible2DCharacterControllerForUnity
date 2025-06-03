using UnityEngine;


public class CrouchState : BaseState
{
	public CrouchState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("CrouchState");
		
		// player.OnEnterCrouch();
		player.playerPhysicsController.CrouchModule.OnEnterCrouch();
		
		// player.HandleGround();
		player.playerPhysicsController.GroundModule.HandleGround();
	}

	public override void FixedUpdate()
	{
		player.HandleMovement();
		player.playerPhysicsController.MovementModule.HandleMovement(player.GetMoveDirection(), player.stats.CrouchMoveSpeed, player.stats.CrouchAcceleration, player.stats.CrouchDeceleration);
	}

	public override void OnExit()
	{
		// player.OnExitCrouch();
		player.playerPhysicsController.CrouchModule.OnExitCrouch(player.input.DashInputButtonState);
	}
}
