using UnityEngine;

public class RunJumpState : BaseState
{
    public RunJumpState(PlayerController player) : base(player) { }
	
    public override void OnEnter()
    {
        Debug.Log("RunJumpState");
    }

    public override void FixedUpdate()
    {		
        // player.HandleJump();
        // player.HandleMovement();
		
        player.playerPhysicsController.HandleJump(player._jumpKeyWasPressed, player._jumpKeyWasLetGo);
        player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.RunSpeed  , player.stats.RunAcceleration, player.stats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
    }

    public override void OnExit()
    {
        // player.OnExitJump();

        player.playerPhysicsController.OnExitJump(); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
    }
}