using System.Net.NetworkInformation;
using UnityEngine;

public class LocomotionState : BaseState
{
	private readonly MovementModule _movementModule;

	public LocomotionState(PlayerStateContext context, MovementModule movementModule) :
		base(context)
	{
		_movementModule = movementModule;
	}
	
	public override void OnEnter()
	{
		Debug.Log("MoveEnter");
		
		animationController.PlayAnimation("Walk");
	}

	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
        
		float currentSpeed = physicsHandler2D.GetVelocity().magnitude;
		animationController.SyncAnimationWithMovement(currentSpeed, playerControllerStats.BaseMovementAnimationSpeed);
	}

	public override void FixedUpdate()
	{
		var moveVelocity = _movementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.MoveSpeed, playerControllerStats.WalkAcceleration, playerControllerStats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		moveVelocity.y = playerControllerStats.GroundGravity;

		physicsHandler2D.AddVelocity(moveVelocity);
	}

	public override void OnExit()
	{
		animationController.SetFloat("Speed", 0f);
	}
}
