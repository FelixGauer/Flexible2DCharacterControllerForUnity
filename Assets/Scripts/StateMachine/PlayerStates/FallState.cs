using UnityEngine;


public class FallState : BaseState
{
	public FallState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		Debug.Log("FallState");
		player.BumpedHead();
	}

	public override void FixedUpdate()
	{
		player.FallForState();
		player.HandleMovement();
	}
}
