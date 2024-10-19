using UnityEngine;


public class LocomotionState : BaseState
{
	public LocomotionState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		player.GroundedState();		
		Debug.Log("MoveEnter");
	}

	public override void FixedUpdate()
	{
		player.HandleMovement();
	}
}
