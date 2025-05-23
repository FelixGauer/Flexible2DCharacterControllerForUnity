using System.Net.NetworkInformation;
using UnityEngine;


public class LocomotionState : BaseState
{
	public LocomotionState(PlayerController player) : base(player)
	{
		// _speed = player.stats.MoveSpeed;
		// _acceleration = player.stats.WalkAcceleration;
		// _deceleration = player.stats.WalkDeceleration;
	}
	
	// private float _speed;
	// private float _acceleration;
	// private float _deceleration;
	
	public override void OnEnter()
	{
		player.HandleGround();		
		Debug.Log("MoveEnter");
	}

	public override void FixedUpdate()
	{
		// player.HandleMovement(); //
		player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed, player.stats.WalkAcceleration, player.stats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
	}
}
