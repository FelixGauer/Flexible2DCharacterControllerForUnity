using UnityEngine;


public class FallState : BaseState
{
	private readonly FallModule _fallModule;
	private readonly MovementModule _movementModule;
	
	private Vector2 _moveVelocity;

	public FallState(PlayerStateContext context, FallModule fallModule, MovementModule movementModule) :
		base(context)
	{
		_fallModule = fallModule;
		_movementModule = movementModule;
	}

	public override void OnEnter()
	{
		Debug.Log("FallState");

		animationController.PlayAnimation("Fall");
	}

	public override void Update()
	{
		_fallModule.BufferJump(inputReader.GetJumpState());
		_fallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{
		// _moveVelocity.y = player.playerPhysicsController.FallModule.HandleFalling(physicsHandler2D.GetLastAppliedVelocity()).y;
		// _moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetLastAppliedVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

		_moveVelocity.y = _fallModule.HandleFalling(physicsHandler2D.GetVelocity()).y;
		_moveVelocity.x = _movementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetNormalizedHorizontalDirection(), playerControllerStats.WalkSpeed, playerControllerStats.AirAcceleration, playerControllerStats.AirDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

		physicsHandler2D.AddVelocity(_moveVelocity);
	}
	
	public override void OnExit()
	{
		
	}
}