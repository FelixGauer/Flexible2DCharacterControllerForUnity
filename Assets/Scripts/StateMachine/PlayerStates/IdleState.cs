using UnityEngine;


public class IdleState : BaseState
{
	public IdleState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		player._moveVelocity.x = 0f;
		player.playerPhysicsController._moveVelocity.x = 0f;
		player.playerPhysicsController._physicsContext.MoveVelocity = Vector2.zero;
		
		
		Debug.Log("IdleState");
		
		
		// player.playerPhysicsController.HandleGround();
		

		// player.playerPhysicsController._jumpModule.HandleGround();
		
		// player.playerPhysicsController.HandleGround2();
		
		
		// player.HandleGround();

		// player.playerPhysicsController._fallModule.OnExitFall();
		
		player.playerPhysicsController.HandleGround2();


	}

	public override void FixedUpdate()
	{
		// UnityEngine.Debug.Log(player.playerPhysicsController._physicsContext.NumberAvailableJumps);
		// player.SMMove();
		// player.HandleMovement();
	}	
}