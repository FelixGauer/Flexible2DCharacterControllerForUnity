using UnityEngine;


public class RunState : BaseState
{
	private readonly MovementModule _movementModule;

	public RunState(PlayerStateContext context, MovementModule movementModule) :
		base(context)
	{
		_movementModule = movementModule;
	}

	public override void OnEnter()
	{		
		Debug.Log("RunState");
		
		animationController.PlayAnimation("Run");
	}
	
	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
		    
		float currentSpeed = physicsHandler2D.GetVelocity().magnitude;
		animationController.SyncAnimationWithMovement(currentSpeed, playerControllerStats.BaseRunAnimationSpeed);
	}
	
	public override void FixedUpdate()
	{

		
		var moveVelocity = _movementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetNormalizedHorizontalDirection(), playerControllerStats.RunSpeed, playerControllerStats.RunAcceleration, playerControllerStats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		physicsHandler2D.AddVelocity(moveVelocity);
	}
	
	public override void OnExit()
	{
		animationController.SetFloat("Speed", 0f);
	}
}
