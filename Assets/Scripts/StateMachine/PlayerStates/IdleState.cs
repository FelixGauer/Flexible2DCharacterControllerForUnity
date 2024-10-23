using UnityEngine;


public class IdleState : BaseState
{
	public IdleState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{		
		Debug.Log("IdleState");
	}

	public override void FixedUpdate()
	{
		// player.SMMove();
	}
}
