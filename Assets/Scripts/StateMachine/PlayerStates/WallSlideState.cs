using UnityEngine;


public class WallSlideState : BaseState
{
	public WallSlideState(PlayerController player, Animator animator) : base(player, animator) { }

	public override void OnEnter()
	{
		animator.Play("WallSlide");

		Debug.Log("WallSlideState");

		// player.OnEnterWallSliding();	
		
		player.playerPhysicsController.WallSlideModule.OnEnterWallSliding();

	}

	public override void FixedUpdate()
	{
		// player.HandleWallInteraction();
		
		// player.playerPhysicsController.WallSlideModule.HandleWallInteraction(player.GetMoveDirection(), player.input.JumpInputButtonState);
		
		player.playerPhysicsController.WallSlideModule.HandleWallSlide(player.GetMoveDirection());
	}

    public override void OnExit()
    {
        // player.OnExitWallSliding();
        
        player.playerPhysicsController.WallSlideModule.OnExitWallSliding();

    }
}