using UnityEngine;

public class IdleCrouchState : BaseState
{
    public IdleCrouchState(PlayerController player) : base(player) { }

    public override void OnEnter()
    {		
        Debug.Log("IdleCrouchState");

        player.playerPhysicsController.CrouchModule.SetCrouchState(true);
        player.playerPhysicsController.GroundModule.HandleGround();
        
        // player.OnEnterCrouch();
        // player.HandleGround();
        
        // player.playerPhysicsController.CrouchModule.OnEnterCrouch();
    }
    
    public override void OnExit()
    {
        if (player.input.DashInputButtonState.WasPressedThisFrame) return;

        player.playerPhysicsController.CrouchModule.SetCrouchState(false);
        
        // player.OnExitCrouch();
        // player.playerPhysicsController.CrouchModule.OnExitCrouch(player.input.DashInputButtonState);
    }
}