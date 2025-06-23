using System.Linq;
using UnityEngine;


public class CrouchRollState : BaseState
{
	private Vector2 _moveVelocity;
	
	public CrouchRollState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }
	
	public override void OnEnter()
	{		
		Debug.Log("CrouchRollState");
		
		animationController.PlayAnimationWithDuration(
			"CrouchRoll", 
			playerControllerStats.CrouchRollTime,
			"CrouchRollSpeedMultiplier"
		);
		
		playerPhysicsController.CrouchRollModule.StartCrouchRoll();
	}
	
	public override void Update()
	{
		// turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}


	public override void FixedUpdate()
	{
		_moveVelocity.x = playerPhysicsController.CrouchRollModule.CrouchRoll(physicsHandler2D.GetVelocity()).x;
		physicsHandler2D.AddVelocity(_moveVelocity);
	}

	public override void OnExit()
	{
		animationController.ResetSpeedParameter("CrouchRollSpeedMultiplier");
		
		playerPhysicsController.CrouchModule.SetCrouchState(false);

		playerPhysicsController.CrouchRollModule.StopCrouchRoll();
	}

}
