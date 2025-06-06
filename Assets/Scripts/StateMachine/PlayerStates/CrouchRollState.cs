using UnityEngine;


public class CrouchRollState : BaseState
{
	public CrouchRollState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("CrouchRollState");

		// player.OnEnterCrouchRoll();
		
		player.playerPhysicsController.CrouchRollModule.StartCrouchRoll();
	}

	public override void FixedUpdate()
	{
		// player.CrouchRoll();
		player.playerPhysicsController.CrouchRollModule.CrouchRoll();

	}

	public override void OnExit()
	{
		// player.playerPhysicsController.CrouchRollModule.OnExitCrouchRoll();

		// player.OnExitCrouchRoll();
		
		player.playerPhysicsController.CrouchRollModule.StopCrouchRoll();

	}
}
