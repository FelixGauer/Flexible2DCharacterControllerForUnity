using UnityEngine;


public class RunState : BaseState
{
	public RunState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }

	public override void OnEnter()
	{		
		Debug.Log("RunState");
		
		animator.Play("Run");

		// player.playerPhysicsController.GroundModule.HandleGround();
		// player.playerPhysicsController.HandleGround();
	}
	
	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}
	
	private Vector2 _moveVelocity;

	public override void FixedUpdate()
	{
		_moveVelocity = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.RunSpeed, playerControllerStats.RunAcceleration, playerControllerStats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		physicsHandler2D.AddVelocity(_moveVelocity);
	}
	
	public override void OnExit()
	{
		// player.playerPhysicsController.CoyoteTimerStart();
	}
}
