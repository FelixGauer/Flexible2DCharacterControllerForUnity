using UnityEngine;

public class JumpState : BaseState
{
	public JumpState(PlayerController player) : base(player) { }
	
	public override void OnEnter()
	{
		Debug.Log("JumpEnter");
	}

	public override void FixedUpdate()
	{
		// player.HandleJump();
		
		player.JumpForState();
		player.HandleMovement();
	}
}