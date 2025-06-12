using UnityEngine;

public class JumpState : BaseState
{
	public JumpState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D) { }
	
	public override void OnEnter()
	{
		animator.Play("Jump");
		
		Debug.Log("JumpEnter");
	}

	public override void Update()
	{
		// if (inputReader.GetJumpState().WasPressedThisFrame && player.playerPhysicsController.PhysicsContext.NumberAvailableJumps == 1)
		// 	animator.Play("MultiJump");
		
		// player.playerPhysicsController.JumpModule.Test1Update(inputReader.GetJumpState());
		player.playerPhysicsController.JumpModule.HandleInput(inputReader.GetJumpState());

	}
	
	private Vector2 _moveVelocity;

	public override void FixedUpdate()
	{
		// _moveVelocity.y = player.playerPhysicsController.JumpModule.Test1FixedUpdate(inputReader.GetJumpState(), physicsHandler2D.GetVelocity()).y;
		_moveVelocity.y = player.playerPhysicsController.JumpModule.UpdatePhysics(inputReader.GetJumpState(), physicsHandler2D.GetVelocity()).y;
		_moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		physicsHandler2D.AddVelocity(_moveVelocity);
	}

    public override void OnExit()
    {
        player.playerPhysicsController.JumpModule.OnExitJump();

    }
	
}