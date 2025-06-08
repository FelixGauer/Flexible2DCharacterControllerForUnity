using System.Net.NetworkInformation;
using UnityEngine;


public class LocomotionState : BaseState
{
	public LocomotionState(PlayerController player, Animator animator) : base(player, animator) { }
	
	public override void OnEnter()
	{
		animator.Play("Run");
		// player.HandleGround();	
		
		player.playerPhysicsController.GroundModule.HandleGround();

		Debug.Log("MoveEnter");
	}

	public override void Update()
	{

	}

	public override void FixedUpdate()
	{
		player.playerPhysicsController.MovementModule.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed, player.stats.WalkAcceleration, player.stats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
	}

	public override void OnExit()
	{
		// player.playerPhysicsController.FallModule.CoyoteTimerStart();  // FIXME
		player.playerPhysicsController.CoyoteTimerStart();
	}
}
