using UnityEngine;

public class IdleCrouchState : BaseState
{
    public IdleCrouchState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
        base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

    public override void OnEnter()
    {		
        Debug.Log("IdleCrouchState");
        
        animationController.PlayAnimation("CrouchIdle");
        


        playerPhysicsController.CrouchModule.SetCrouchState(true);
        // player.playerPhysicsController.GroundModule.HandleGround();
        
        // player.playerPhysicsController.PhysicsContext.MoveVelocity = Vector2.zero;
        // player.playerPhysicsController.CrouchModule.OnEnterCrouch();
    }

    public override void Update()
    {
        turnChecker.TurnCheck(inputReader.GetMoveDirection());
    }

    public override void OnExit()
    {
        if (inputReader.GetDashState().WasPressedThisFrame) return;

        playerPhysicsController.CrouchModule.SetCrouchState(false);
        
        // player.playerPhysicsController.CrouchModule.OnExitCrouch(player.input.DashInputButtonState);
    }
    

}