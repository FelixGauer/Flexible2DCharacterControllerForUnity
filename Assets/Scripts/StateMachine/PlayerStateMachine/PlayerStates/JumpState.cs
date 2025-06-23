using UnityEngine;

public class JumpState : BaseState
{
	private Vector2 _moveVelocity;

	public JumpState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }
	
	public override void OnEnter()
	{
		Debug.Log("JumpEnter");

		animationController.PlayAnimation("Jump");
		
		playerPhysicsController.JumpModule.OnMultiJump += () => animationController.PlayAnimation("MultiJump");
	}

	public override void Update()
	{
		playerPhysicsController.JumpModule.HandleInput(inputReader.GetJumpState());
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}
	

	public override void FixedUpdate()
	{
		_moveVelocity.y = playerPhysicsController.JumpModule.UpdatePhysics(inputReader.GetJumpState(), physicsHandler2D.GetVelocity()).y;
		_moveVelocity.x = playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		// physicsHandler2D.AddVelocity(_moveVelocity);
		
		var gravity = playerPhysicsController.FallModule.ApplyGravity(_moveVelocity, playerControllerStats.Gravity, playerControllerStats.JumpGravityMultiplayer);
		physicsHandler2D.AddVelocity(gravity);
	}

    public override void OnExit()
    {
        playerPhysicsController.JumpModule.OnExitJump();
    }
}