using UnityEngine;

public class JumpState : BaseState
{
	private Vector2 _moveVelocity;

	public JumpState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }
	
	public override void OnEnter()
	{
		animator.Play("Jump");
		
		Debug.Log("JumpEnter");
		
		player.playerPhysicsController.JumpModule.OnMultiJump += () => animator.Play("MultiJump");
	}

	public override void Update()
	{
		player.playerPhysicsController.JumpModule.HandleInput(inputReader.GetJumpState());
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}
	

	public override void FixedUpdate()
	{
		_moveVelocity.y = player.playerPhysicsController.JumpModule.UpdatePhysics(inputReader.GetJumpState(), physicsHandler2D.GetVelocity()).y;
		_moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		// physicsHandler2D.AddVelocity(_moveVelocity);
		
		var gravity = player.playerPhysicsController.FallModule.ApplyGravity(_moveVelocity, playerControllerStats.Gravity, playerControllerStats.JumpGravityMultiplayer);
		physicsHandler2D.AddVelocity(gravity);
	}

    public override void OnExit()
    {
        player.playerPhysicsController.JumpModule.OnExitJump();
    }
}