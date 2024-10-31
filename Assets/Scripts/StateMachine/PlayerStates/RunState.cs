using UnityEngine;


public class RunState : BaseState
{
	public RunState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("RunState");
		player.OnEnterRun();
	}

	public override void FixedUpdate()
	{
		player.HandleMovement();
	}
}
