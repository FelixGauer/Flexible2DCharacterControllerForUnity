using UnityEngine;


public class DashState : BaseState
{
	// private Vector2 _moveVelocity;

	public DashState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

	public override void OnEnter()
	{		
		animator.Play("Dash");

		Debug.Log("DashState");
		
		player.playerPhysicsController.DashModule.StartDash(inputReader.GetMoveDirection());
	}

	public override void Update()
	{
		player.playerPhysicsController.DashModule.CheckForDirectionChange(inputReader.GetMoveDirection());
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{
		var moveVelocity = player.playerPhysicsController.DashModule.HandleDash();
		var gravityVelocity = player.playerPhysicsController.FallModule.ApplyGravity(Vector2.zero, playerControllerStats.Gravity, playerControllerStats.DashGravityMultiplayer).y;
		
		physicsHandler2D.AddVelocity(moveVelocity);
		physicsHandler2D.AddVelocity(new Vector2(0f, gravityVelocity));
	}
	
	public override void OnExit()
	{
		player.playerPhysicsController.DashModule.StopDash();
	}
}
