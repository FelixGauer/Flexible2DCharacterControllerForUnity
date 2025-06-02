using UnityEngine;


public class FallState : BaseState
{
	public FallState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		Debug.Log("FallState");
		player.BumpedHead(); // FIXME
		// player.CoyoteTimerStart();
		
		// player.playerPhysicsController.CoyoteTimerStart();

		// player.playerPhysicsController.FallModule.CoyoteTimerStart();  // FIXME
	}

	public override void Update()
	{
		// player.playerPhysicsController.FallModule.Test(player.input.JumpInputButtonState);

	}

	public override void FixedUpdate()
	{
		// player.HandleFalling();
		// player.HandleMovement();
		
		// player.playerPhysicsController.HandleFalling(player._jumpKeyWasPressed, player._jumpKeyWasLetGo, player._jumpKeyIsPressed);
		// player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed, player.stats.airAcceleration, player.stats.airDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		
		player.playerPhysicsController.FallModule.HandleFalling(player.input.JumpInputButtonState);
		player.playerPhysicsController.MovementModule.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed, player.stats.airAcceleration, player.stats.airDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

	}
	
	public override void OnExit()
	{
		// player.OnExitFall();

		// player.playerPhysicsController.OnExitFall();
		
		// player.playerPhysicsController.HandleGround2();
		
		player.playerPhysicsController.FallModule.OnExitFall(); // FIXME
		
	}
	
}