using UnityEngine;


public class DashState : BaseState
{
	public DashState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("DashState");
	}

	public override void FixedUpdate()
	{
		player.HandleDash();
	}
}
