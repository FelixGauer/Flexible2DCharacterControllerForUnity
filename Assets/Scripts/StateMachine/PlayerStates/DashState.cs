using UnityEngine;


public class DashState : BaseState
{
	public DashState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("DashState");
		// player.OnEnterDash();
		// player.CalculateDashDirection();
		
		
		player.playerPhysicsController.DashModule.OnEnterDash();
		player.playerPhysicsController.DashModule.CalculateDashDirection(player.input.Direction);
	}

	public override void FixedUpdate()
	{
		// player.HandleDash();
		
		player.playerPhysicsController.DashModule.HandleDash(player.GetMoveDirection());
	}
	
	public override void OnExit()
	{
		// player.OnExitDash();
		
		player.playerPhysicsController.DashModule.OnExitDash();
	}
}
