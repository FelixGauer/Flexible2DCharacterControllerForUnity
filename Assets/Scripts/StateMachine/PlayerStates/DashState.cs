using UnityEngine;


public class DashState : BaseState
{
	public DashState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("DashState");
		// player.OnEnterDash();
		// player.CalculateDashDirection();
		
		
		player.playerPhysicsController._dashModule.OnEnterDash();
		player.playerPhysicsController._dashModule.CalculateDashDirection(player.input);
	}

	public override void FixedUpdate()
	{
		// player.HandleDash();
		
		player.playerPhysicsController.HandleDash(player.GetMoveDirection());
	}
	
	public override void OnExit()
	{
		// player.OnExitDash();
		
		player.playerPhysicsController._dashModule.OnExitDash();
	}
}
