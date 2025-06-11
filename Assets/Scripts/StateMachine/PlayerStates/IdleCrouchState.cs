using UnityEngine;

public class IdleCrouchState : BaseState
{
    public IdleCrouchState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D) :
        base(player, animator, inputReader, playerControllerStats, physicsHandler2D) { }

    public override void OnEnter()
    {		
        Debug.Log("IdleCrouchState");
        
        animator.Play("CrouchIdle");


        player.playerPhysicsController.CrouchModule.SetCrouchState(true);
        player.playerPhysicsController.GroundModule.HandleGround();
        
        // player.playerPhysicsController.PhysicsContext.MoveVelocity = Vector2.zero;
        // player.playerPhysicsController.CrouchModule.OnEnterCrouch();
    }
    
    public override void OnExit()
    {
        if (inputReader.GetDashState().WasPressedThisFrame) return;

        player.playerPhysicsController.CrouchModule.SetCrouchState(false);
        
        // player.playerPhysicsController.CrouchModule.OnExitCrouch(player.input.DashInputButtonState);
    }
    

}