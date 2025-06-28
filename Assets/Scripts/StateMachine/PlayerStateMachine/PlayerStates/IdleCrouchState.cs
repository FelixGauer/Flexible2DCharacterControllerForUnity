using UnityEngine;

public class IdleCrouchState : BaseState
{
    private readonly CrouchModule _crouchModule;

    public IdleCrouchState(PlayerStateContext context, CrouchModule crouchModule) :
        base(context)
    {
        _crouchModule = crouchModule;
    }

    public override void OnEnter()
    {		
        Debug.Log("IdleCrouchState");
        
        animationController.PlayAnimation("CrouchIdle");
        
        _crouchModule.SetCrouchState(true);

    }

    public override void Update()
    {
        // Debug.Log(turnChecker.IsFacingRight);

        turnChecker.TurnCheck(inputReader.GetMoveDirection());
    }

    public override void OnExit()
    {
        if (inputReader.GetDashState().WasPressedThisFrame) return;

        _crouchModule.SetCrouchState(false);
        
        // player.playerPhysicsController.CrouchModule.OnExitCrouch(player.input.DashInputButtonState);
    }
    

}