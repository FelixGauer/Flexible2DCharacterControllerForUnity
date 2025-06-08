using UnityEngine;

public class RunJumpState : BaseState
{
    public RunJumpState(PlayerController player, Animator animator) : base(player, animator) { }
	
    public override void OnEnter()
    {
        animator.Play("Jump");

        Debug.Log("RunJumpState");
    }
    
    public override void Update()
    {
        player.playerPhysicsController.JumpModule.Test1Update(player.input.JumpInputButtonState);
    }

    public override void FixedUpdate()
    {		
        // player.HandleJump();
        // player.HandleMovement();
		
        // player.playerPhysicsController.HandleJump(player._jumpKeyWasPressed, player._jumpKeyWasLetGo);
        // player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.RunSpeed  , player.stats.RunAcceleration, player.stats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        
        // player.playerPhysicsController.JumpModule.HandleJump(player.input.JumpInputButtonState);

        
        player.playerPhysicsController.JumpModule.Test1FixedUpdate(player.input.JumpInputButtonState);
        player.playerPhysicsController.MovementModule.HandleMovement(player.GetMoveDirection(), player.stats.RunSpeed, player.stats.RunAcceleration, player.stats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection

    }

    public override void OnExit()
    {
        // player.OnExitJump();

        // player.playerPhysicsController.OnExitJump(); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        
        player.playerPhysicsController.JumpModule.OnExitJump(); // FIXME
    }
}