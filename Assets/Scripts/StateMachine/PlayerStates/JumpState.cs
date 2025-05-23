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
		// player.HandleMovement();
		
		player.playerPhysicsController.HandleJump(player._jumpKeyWasPressed, player._jumpKeyWasLetGo);
		player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed  , player.stats.WalkAcceleration, player.stats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
	}

    public override void OnExit()
    {
        // player.OnExitJump();

        player.playerPhysicsController.OnExitJump(); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

    }
	
}