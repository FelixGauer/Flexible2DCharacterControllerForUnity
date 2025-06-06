using UnityEngine;


public class RunState : BaseState
{
	public RunState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("RunState");
		// player.OnEnterRun();
		// player.HandleGround();
		
		// player.playerPhysicsController.HandleGround();

		player.playerPhysicsController.GroundModule.HandleGround();
	}

	public override void FixedUpdate()
	{
		// player.HandleMovement();
		player.playerPhysicsController.MovementModule.HandleMovement(player.GetMoveDirection(), player.stats.RunSpeed, player.stats.RunAcceleration, player.stats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
	}
	
	public override void OnExit()
	{
		// player.playerPhysicsController.FallModule.CoyoteTimerStart();  // FIXME
		player.playerPhysicsController.CoyoteTimerStart();

	}
}
