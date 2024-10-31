using UnityEngine;


public class CrouchRollState : BaseState
{
	public CrouchRollState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		player.OnEnterCrouchRoll();
		Debug.Log("CrouchRollState");
	}

	public override void FixedUpdate()
	{
		// player.SMMove();
		// player.Crouch();
		player.CrouchRoll();
	}

	public override void OnExit()
	{
		player.OnExitCrouchRoll();
	}
}
