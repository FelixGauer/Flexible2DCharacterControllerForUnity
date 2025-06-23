using UnityEngine;


public class DashState : BaseState
{
	// private Vector2 _moveVelocity;

	public DashState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

	public override void OnEnter()
	{		
		Debug.Log("DashState");

		animationController.PlayAnimation("Dash");
		
		playerPhysicsController.DashModule.StartDash(inputReader.GetMoveDirection());
	}

	public override void Update()
	{
		playerPhysicsController.DashModule.CheckForDirectionChange(inputReader.GetMoveDirection());
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{
		var moveVelocity = playerPhysicsController.DashModule.HandleDash();
		var gravityVelocity = playerPhysicsController.FallModule.ApplyGravity(Vector2.zero, playerControllerStats.Gravity, playerControllerStats.DashGravityMultiplayer).y;
		
		physicsHandler2D.AddVelocity(moveVelocity);
		physicsHandler2D.AddVelocity(new Vector2(0f, gravityVelocity));
	}
	
	public override void OnExit()
	{
		playerPhysicsController.DashModule.StopDash();
	}
}
