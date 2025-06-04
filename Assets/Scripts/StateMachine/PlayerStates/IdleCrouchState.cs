using UnityEngine;

public class IdleCrouchState : BaseState
{
    public IdleCrouchState(PlayerController player) : base(player) { }

    public override void OnEnter()
    {		
        Debug.Log("IdleCrouchState");
        // player.OnEnterCrouch();

        // player.HandleGround();
        
        player.playerPhysicsController.CrouchModule.OnEnterCrouch();
        player.playerPhysicsController.GroundModule.HandleGround();

    }

    public override void FixedUpdate()
    {
        // player.SMMove();
        // player.HandleMovement();
    }
    
    public override void OnExit()
    {
        // player.OnExitCrouch();
        
        player.playerPhysicsController.CrouchModule.OnExitCrouch(player.input.DashInputButtonState);

    }
}