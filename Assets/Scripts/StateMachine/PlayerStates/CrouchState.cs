using UnityEngine;


public class CrouchState : BaseState
{
	public CrouchState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("CrouchState");
		player.OnEnterCrouch();
	}

	public override void FixedUpdate()
	{
		player.HandleMovement();
		// player.Crouch();
	}

	public override void OnExit()
	{
		player.OnExitCrouch();
	}
}
