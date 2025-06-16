using UnityEngine;

public class WallJumpState : BaseState
{
    public WallJumpState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
        base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }

    private Vector2 _moveVelocity;
    public override void OnEnter()
    {
        Debug.Log("WallJumpState");
        _moveVelocity = player.playerPhysicsController.WallJumpModule.HandleWallJump(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection());
        physicsHandler2D.AddVelocity(_moveVelocity);
    }

    public override void FixedUpdate()
    {
    }

    public override void OnExit()
    {
        inputReader.ResetFrameStates();
    }
}