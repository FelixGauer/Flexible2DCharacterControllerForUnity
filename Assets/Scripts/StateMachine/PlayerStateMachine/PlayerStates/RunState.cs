using UnityEngine;


public class RunState : BaseState
{
	public RunState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

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
		var moveVelocity = playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.RunSpeed, playerControllerStats.RunAcceleration, playerControllerStats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		physicsHandler2D.AddVelocity(moveVelocity);
	}
	
	public override void OnExit()
	{
		animationController.SetFloat("Speed", 0f);
	}
}
