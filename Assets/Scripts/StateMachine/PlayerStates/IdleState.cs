using UnityEngine;


public class IdleState : BaseState
{
	public IdleState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		player._moveVelocity.x = 0f;
		player.playerPhysicsController._moveVelocity.x = 0f;

		Debug.Log("IdleState");
		
		// player.HandleGround();
		
		player.playerPhysicsController.HandleGround();

	}

	public override void FixedUpdate()
	{
		// player.SMMove();
		// player.HandleMovement();
	}	
}