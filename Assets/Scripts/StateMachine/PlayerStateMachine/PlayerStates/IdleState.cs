using UnityEngine;


public class IdleState : BaseState
{
	public IdleState(PlayerPhysicsController playerPhysicsController, InputReader inputReader,
		PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

	public override void OnEnter()
	{
		Debug.Log("IdleState");

		animationController.PlayAnimation("Idle");
	}

	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{

	}	
}