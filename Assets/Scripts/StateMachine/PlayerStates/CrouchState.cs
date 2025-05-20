using UnityEngine;


public class CrouchState : BaseState
{
	public CrouchState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("CrouchState");
		player.OnEnterCrouch();
		
		// player.SetCrouchState(true);
		
		player.HandleGround();
	}

	public override void FixedUpdate()
	{
		player.HandleMovement();
	}

	public override void OnExit()
	{
		player.OnExitCrouch();
	}
}
