using UnityEngine;


public class CrouchState : BaseState
{
	public CrouchState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

	public override void OnEnter()
	{		
		Debug.Log("CrouchState");
		
		animationController.PlayAnimation("CrouchWalk");
		
		playerPhysicsController.CrouchModule.SetCrouchState(true);
	}

	private Vector2 _moveVelocity;
	
	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{
		_moveVelocity = playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.CrouchMoveSpeed, playerControllerStats.CrouchAcceleration, playerControllerStats.CrouchDeceleration);
		physicsHandler2D.AddVelocity(_moveVelocity);
	}

	public override void OnExit()
	{
		if (inputReader.GetDashState().WasPressedThisFrame) return;
		
		playerPhysicsController.CrouchModule.SetCrouchState(false);

		// player.OnExitCrouch();
		// player.playerPhysicsController.CrouchModule.OnExitCrouch(player.input.DashInputButtonState);
	}
}
