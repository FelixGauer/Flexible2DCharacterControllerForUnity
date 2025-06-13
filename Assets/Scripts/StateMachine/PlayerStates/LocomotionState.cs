using System.Net.NetworkInformation;
using UnityEngine;

public class LocomotionState : BaseState
{
	// TODO Заменить PlayerController player на playerPhysicsController
	
	public LocomotionState(PlayerController player, Animator animator, InputReader inputReader,
		PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker)
	{
		// _movementModule = new MovementModule(player.playerPhysicsController.PhysicsContext, );
		// _groundModule = new GroundModule(playerControllerStats, player.playerPhysicsController.PhysicsContext);
	}

	private MovementModule _movementModule;
	private GroundModule _groundModule;


	public override void OnEnter()
	{
		Debug.Log("MoveEnter");

		animator.Play("Run");
		
		player.playerPhysicsController.GroundModule.HandleGround();
		// _groundModule.HandleGround();

	}

	public override void Update()
	{
		// turnChecker.TurnCheck(inputReader.GetMoveDirection());
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{
		// _movementModule.HandleMovement(inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.WalkAcceleration, playerControllerStats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		var moveVelocity = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.WalkAcceleration, playerControllerStats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		physicsHandler2D.AddVelocity(moveVelocity);
	}

	public override void OnExit()
	{
		player.playerPhysicsController.CoyoteTimerStart();
	}
}
