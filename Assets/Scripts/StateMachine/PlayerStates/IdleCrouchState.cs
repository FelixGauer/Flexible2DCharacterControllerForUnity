using UnityEngine;

public class IdleCrouchState : BaseState
{
    public IdleCrouchState(PlayerController player) : base(player) { }

    public override void OnEnter()
    {		
        Debug.Log("IdleCrouchState");
        player.OnEnterCrouch();

        player.HandleGround();
    }

    public override void FixedUpdate()
    {
        // player.SMMove();
        player.HandleMovement();
    }
    
    public override void OnExit()
    {
        player.OnExitCrouch();
    }
}