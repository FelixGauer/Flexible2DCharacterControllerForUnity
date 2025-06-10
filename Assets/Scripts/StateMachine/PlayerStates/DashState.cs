using UnityEngine;


public class DashState : BaseState
{
	public DashState(PlayerController player, Animator animator) : base(player, animator) { }

	public override void OnEnter()
	{		
		animator.Play("Dash");

		Debug.Log("DashState");
		// player.OnEnterDash();
		// player.CalculateDashDirection();
		
		player.playerPhysicsController.DashModule.StartDash(player.input.GetMoveDirection());
		// player.playerPhysicsController.DashModule.CalculateDashDirection(player.input.Direction);
	}

	public override void Update()
	{
		if (player.playerPhysicsController.DashModule.CurrentDashDirection == -player.input.GetMoveDirection())
			player.playerPhysicsController.DashModule.StopDash(); // TODO Вынести как отдельный метод в DashModule
	}

	public override void FixedUpdate()
	{
		// player.HandleDash();
		
		player.playerPhysicsController.DashModule.HandleDash(player.input.GetMoveDirection());
	}
	
	public override void OnExit()
	{
		// player.OnExitDash();
		
		player.playerPhysicsController.DashModule.StopDash();
	}
}
