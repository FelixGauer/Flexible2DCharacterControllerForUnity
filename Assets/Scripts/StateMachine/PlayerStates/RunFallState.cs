using UnityEngine;

public class RunFallState : BaseState
{
    public RunFallState(PlayerController player) : base(player) { }

    public override void OnEnter()
    {
        Debug.Log("RunFallState");
        player.BumpedHead(); // FIXME
        player.CoyoteTimerStart();
        
        // player.playerPhysicsController.CoyoteTimerStart();
        player.playerPhysicsController._fallModule.CoyoteTimerStart();

    }

    public override void FixedUpdate()
    {
        // player.HandleFalling();
        // player.HandleMovement();
		
        // player.playerPhysicsController.HandleFalling(player._jumpKeyWasPressed, player._jumpKeyWasLetGo, player._jumpKeyIsPressed);
        
        player.playerPhysicsController.HandleFalling();
        player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.RunSpeed, player.stats.airAcceleration, player.stats.airDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
    }
	
    public override void OnExit()
    {
        // player.OnExitFall();

        // player.playerPhysicsController.OnExitFall();
        
        player.playerPhysicsController._fallModule.OnExitFall(); // FIXME

    }
	
}