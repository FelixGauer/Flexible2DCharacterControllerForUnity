using UnityEngine;


public class FallState : BaseState
{
	private Vector2 _moveVelocity;

	public FallState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

	public override void OnEnter()
	{
		animator.Play("Fall");

		Debug.Log("FallState");
	}

	public override void Update()
	{
		player.playerPhysicsController.FallModule.BufferJump(inputReader.GetJumpState());
		player.playerPhysicsController.FallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{
		// _moveVelocity.y = player.playerPhysicsController.FallModule.HandleFalling(physicsHandler2D.GetLastAppliedVelocity()).y;
		// _moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetLastAppliedVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

		_moveVelocity.y = player.playerPhysicsController.FallModule.HandleFalling(physicsHandler2D.GetVelocity()).y;
		_moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

		physicsHandler2D.AddVelocity(_moveVelocity);
	}
	
	public override void OnExit()
	{
		
	}
}