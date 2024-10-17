using UnityEngine;


public class LocomotionState : BaseState
{
	public LocomotionState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		Debug.Log("MoveEnter");
	}

	public override void FixedUpdate()
	{
		player.SMMove();
	}
}
