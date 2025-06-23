using UnityEngine;


public class FallState : BaseState
{
	private Vector2 _moveVelocity;

	public FallState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

	public override void OnEnter()
	{
		Debug.Log("FallState");

		animationController.PlayAnimation("Fall");
	}

	public override void Update()
	{
		playerPhysicsController.FallModule.BufferJump(inputReader.GetJumpState());
		playerPhysicsController.FallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{
		// _moveVelocity.y = player.playerPhysicsController.FallModule.HandleFalling(physicsHandler2D.GetLastAppliedVelocity()).y;
		// _moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetLastAppliedVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

		_moveVelocity.y = playerPhysicsController.FallModule.HandleFalling(physicsHandler2D.GetVelocity()).y;
		_moveVelocity.x = playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

		physicsHandler2D.AddVelocity(_moveVelocity);
	}
	
	public override void OnExit()
	{
		
	}
}