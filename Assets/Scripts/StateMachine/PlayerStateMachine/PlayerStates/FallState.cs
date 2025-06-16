using UnityEngine;


public class FallState : BaseState
{
	public FallState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }

	public override void OnEnter()
	{
		animator.Play("Fall");

		Debug.Log("FallState");
		
		// player.playerPhysicsController.FallModule.CoyoteTimerStart();  // FIXME
	}

	public override void Update()
	{
		// player.playerPhysicsController.FallModule.Test(player.input.JumpInputButtonState);
		
		player.playerPhysicsController.FallModule.BufferJump(inputReader.GetJumpState());
		player.playerPhysicsController.FallModule.RequestVariableJump(inputReader.GetJumpState());
		player.playerPhysicsController.FallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	private Vector2 _moveVelocity;

	public override void FixedUpdate()
	{
		// player.HandleFalling();
		// player.HandleMovement();
		
		// player.playerPhysicsController.HandleFalling(player._jumpKeyWasPressed, player._jumpKeyWasLetGo, player._jumpKeyIsPressed);
		// player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed, player.stats.airAcceleration, player.stats.airDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		
		// player.playerPhysicsController.FallModule.HandleFalling(player.input.JumpInputButtonState);
		
		
		_moveVelocity.y = player.playerPhysicsController.FallModule.HandleFalling(physicsHandler2D.GetVelocity()).y;
		_moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		physicsHandler2D.AddVelocity(_moveVelocity);
	}
	
	public override void OnExit()
	{
		// player.OnExitFall();
	
		// player.playerPhysicsController.OnExitFall();
		
		// player.playerPhysicsController.HandleGround2();
		

		
		player.playerPhysicsController.FallModule.OnExitFall(); // FIXME
		
		// player.playerPhysicsController.MovementModule.ResetMoveVelocity();

		
	}
}