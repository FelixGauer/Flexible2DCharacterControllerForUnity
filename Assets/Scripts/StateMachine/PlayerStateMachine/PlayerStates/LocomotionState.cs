using System.Net.NetworkInformation;
using UnityEngine;

public class LocomotionState : BaseState
{
	// TODO Заменить PlayerController player на playerPhysicsController
	
	public LocomotionState(PlayerController player, Animator animator, InputReader inputReader,
		PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }

	public override void OnEnter()
	{
		Debug.Log("MoveEnter");

		animator.Play("Run");
	}

	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{
		var moveVelocity = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.WalkAcceleration, playerControllerStats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		moveVelocity.y = playerControllerStats.GroundGravity;
		physicsHandler2D.AddVelocity(moveVelocity);
	}

	public override void OnExit()
	{
	}
}
