using UnityEngine;

public class JumpState : BaseState
{
	private readonly JumpModule _jumpModule;
	private readonly FallModule _fallModule;
	private readonly MovementModule _movementModule;
	
	private Vector2 _moveVelocity;

	public JumpState(PlayerStateContext context, JumpModule jumpModule, FallModule fallModule, MovementModule movementModule) :
		base(context)
	{
		_jumpModule = jumpModule;
		_fallModule = fallModule;
		_movementModule = movementModule;
	}
	
	public override void OnEnter()
	{
		Debug.Log("JumpEnter");

		animationController.PlayAnimation("Jump");
		
		_jumpModule.OnMultiJump += () => animationController.PlayAnimation("MultiJump");
	}

	public override void Update()
	{
		_jumpModule.HandleInput(inputReader.GetJumpState());
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}
	

	public override void FixedUpdate()
	{
		// _moveVelocity.y = _jumpModule.UpdatePhysics(inputReader.GetJumpState(), physicsHandler2D.GetVelocity()).y;
		_moveVelocity.y = _jumpModule.JumpPhysicsProcessing(physicsHandler2D.GetVelocity()).y;

		_moveVelocity.x = _movementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		// physicsHandler2D.AddVelocity(_moveVelocity);
		
		// var gravity = _fallModule.ApplyGravity(_moveVelocity, playerControllerStats.Gravity, playerControllerStats.JumpGravityMultiplayer);
		_moveVelocity.y = _fallModule.HandleFalling(_moveVelocity).y;

		physicsHandler2D.AddVelocity(_moveVelocity);
	}

    public override void OnExit()
    {
	    _jumpModule.OnExitJump();
    }
}