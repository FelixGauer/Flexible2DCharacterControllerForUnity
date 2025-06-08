using UnityEngine;


public class CrouchState : BaseState
{
	public CrouchState(PlayerController player, Animator animator) : base(player, animator) { }

	public override void OnEnter()
	{		
		Debug.Log("CrouchState");
		
		animator.Play("CrouchWalk");
		
		player.playerPhysicsController.CrouchModule.SetCrouchState(true);
		
		player.playerPhysicsController.GroundModule.HandleGround();
	}

	public override void FixedUpdate()
	{
		player.playerPhysicsController.MovementModule.HandleMovement(player.GetMoveDirection(), player.stats.CrouchMoveSpeed, player.stats.CrouchAcceleration, player.stats.CrouchDeceleration);
	}

	public override void OnExit()
	{
		if (player.input.DashInputButtonState.WasPressedThisFrame) return;
		
		player.playerPhysicsController.CrouchModule.SetCrouchState(false);

		// player.OnExitCrouch();
		// player.playerPhysicsController.CrouchModule.OnExitCrouch(player.input.DashInputButtonState);
	}
}
