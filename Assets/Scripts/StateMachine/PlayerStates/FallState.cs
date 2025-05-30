using UnityEngine;


public class FallState : BaseState
{
	public FallState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		Debug.Log("FallState");
		player.BumpedHead(); // FIXME
		player.CoyoteTimerStart();
		
		// player.playerPhysicsController.CoyoteTimerStart();

		player.playerPhysicsController._fallModule.CoyoteTimerStart();  // FIXME
	}

	public override void FixedUpdate()
	{
		// player.HandleFalling();
		// player.HandleMovement();
		
		// player.playerPhysicsController.HandleFalling(player._jumpKeyWasPressed, player._jumpKeyWasLetGo, player._jumpKeyIsPressed);
		// player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed, player.stats.airAcceleration, player.stats.airDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		
		player.playerPhysicsController.HandleFalling();
		player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed, player.stats.airAcceleration, player.stats.airDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

	}
	
	public override void OnExit()
	{
		// player.OnExitFall();

		// player.playerPhysicsController.OnExitFall();
		
		// player.playerPhysicsController.HandleGround2();
		
		player.playerPhysicsController._fallModule.OnExitFall(); // FIXME
		
	}
	
}