using UnityEngine;


public class RunState : BaseState
{
	public RunState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("RunState");
		player.OnEnterRun();
		player.HandleGround();
		
		player.playerPhysicsController.HandleGround();
	}

	public override void FixedUpdate()
	{
		// player.HandleMovement();
		player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.RunSpeed, player.stats.RunAcceleration, player.stats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
	}
}
