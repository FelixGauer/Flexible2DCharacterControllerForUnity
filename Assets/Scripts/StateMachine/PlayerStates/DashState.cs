using UnityEngine;


public class DashState : BaseState
{
	public DashState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("DashState");
		player.OnEnterDash();
		player.CalculateDashDirection();
	}

	public override void FixedUpdate()
	{
		player.HandleDash();
	}
	
	public override void OnExit()
	{
		player.OnExitDash();
	}
}
