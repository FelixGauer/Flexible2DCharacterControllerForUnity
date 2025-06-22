using System.Net.NetworkInformation;
using UnityEngine;

public class LocomotionState : BaseState
{
	public LocomotionState(PlayerController player, Animator animator, InputReader inputReader,
		PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }
	
	public override void OnEnter()
	{
		Debug.Log("MoveEnter");
		
		animator.Play("Walk");
	}

	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
        
		float currentSpeed = physicsHandler2D.GetVelocity().magnitude;
		animationController.SyncAnimationWithMovement(currentSpeed, playerControllerStats.BaseMovementAnimationSpeed);

	}

	public override void FixedUpdate()
	{
		var moveVelocity = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.WalkAcceleration, playerControllerStats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		moveVelocity.y = playerControllerStats.GroundGravity;
		physicsHandler2D.AddVelocity(moveVelocity);
	}

	public override void OnExit()
	{
		animationController.SetFloat("Speed", 0f);
	}
}
