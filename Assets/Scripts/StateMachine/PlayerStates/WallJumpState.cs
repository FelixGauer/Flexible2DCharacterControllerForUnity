using UnityEngine;


public class WallJumpState : BaseState
{
	public WallJumpState(PlayerController player) : base(player) { }

	public override void OnEnter()
	{
		player.OnEnterWallSliding();		
		Debug.Log("WallJumpState");
	}

	public override void FixedUpdate()
	{
		player.HandleWallInteraction();
	}

    public override void OnExit()
    {
        player.OnExitWallSliding();
    }
}
