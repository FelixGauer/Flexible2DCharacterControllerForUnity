using UnityEngine;


public class RunState : BaseState
{
	public RunState(PlayerController player, Animator animator) : base(player, animator) { }

	public override void OnEnter()
	{		
		Debug.Log("RunState");
		
		animator.Play("Run");

		player.playerPhysicsController.GroundModule.HandleGround();
	}
	
	public override void Update()
	{
	}

	public override void FixedUpdate()
	{
		player.playerPhysicsController.MovementModule.HandleMovement(player.input.GetMoveDirection(), player.stats.RunSpeed, player.stats.RunAcceleration, player.stats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
	}
	
	public override void OnExit()
	{
		player.playerPhysicsController.CoyoteTimerStart();
	}
}
