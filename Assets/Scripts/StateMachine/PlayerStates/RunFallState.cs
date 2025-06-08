using UnityEngine;

public class RunFallState : BaseState
{
    public RunFallState(PlayerController player, Animator animator) : base(player, animator) { }

    public override void OnEnter()
    {
        Debug.Log("RunFallState");
        
        
        animator.Play("Fall");


        // player.playerPhysicsController.FallModule.CoyoteTimerStart();
    }
    
    public override void Update()
    {
        // player.playerPhysicsController.FallModule.Test(player.input.JumpInputButtonState);
        
        player.playerPhysicsController.FallModule.BufferJump(player.input.JumpInputButtonState);
        player.playerPhysicsController.FallModule.RequestVariableJump(player.input.JumpInputButtonState);
        player.playerPhysicsController.FallModule.SetHoldState(player.input.JumpInputButtonState.IsHeld);
    }

    public override void FixedUpdate()
    {
        // player.HandleFalling();
        // player.HandleMovement();
		
        // player.playerPhysicsController.HandleFalling(player._jumpKeyWasPressed, player._jumpKeyWasLetGo, player._jumpKeyIsPressed);
        
        // player.playerPhysicsController.FallModule.HandleFalling(player.input.JumpInputButtonState);
        player.playerPhysicsController.FallModule.HandleFalling();
        player.playerPhysicsController.MovementModule.HandleMovement(player.GetMoveDirection(), player.stats.RunSpeed, player.stats.airAcceleration, player.stats.airDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
    }
	
    public override void OnExit()
    {
        // player.OnExitFall();

        // player.playerPhysicsController.OnExitFall();
        
        // player.playerPhysicsController.FallModule.OnExitFall(); // FIXME

    }
	
}