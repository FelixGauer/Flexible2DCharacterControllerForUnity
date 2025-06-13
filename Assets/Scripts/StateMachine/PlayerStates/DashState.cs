using UnityEngine;


public class DashState : BaseState
{
	public DashState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }

	public override void OnEnter()
	{		
		animator.Play("Dash");

		Debug.Log("DashState");
		// player.OnEnterDash();
		// player.CalculateDashDirection();
		
		player.playerPhysicsController.DashModule.StartDash(inputReader.GetMoveDirection());
		// player.playerPhysicsController.DashModule.CalculateDashDirection(player.input.Direction);
	}

	public override void Update()
	{
		if (player.playerPhysicsController.DashModule.CurrentDashDirection == -inputReader.GetMoveDirection())
			player.playerPhysicsController.DashModule.StopDash(); // TODO Вынести как отдельный метод в DashModule
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	private Vector2 _moveVelocity;
	public override void FixedUpdate()
	{
		// player.HandleDash();
		
		_moveVelocity = player.playerPhysicsController.DashModule.HandleDash();
		physicsHandler2D.AddVelocity(_moveVelocity);
	}
	
	public override void OnExit()
	{
		// player.OnExitDash();
		
		player.playerPhysicsController.DashModule.StopDash();
	}
}
