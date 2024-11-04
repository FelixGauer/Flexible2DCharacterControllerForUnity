using UnityEngine;


public class FallState : BaseState
{
	public FallState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		Debug.Log("FallState");
		player.BumpedHead(); // FIXME
		player.CoyoteTimerStart();
	}

	public override void FixedUpdate()
	{
		player.HandleFalling();
		player.HandleMovement();
	}
	
	public override void OnExit()
	{
		player.OnExitFall();
	}
	
}
