using UnityEngine;

public class RunFallState : BaseState
{
    public RunFallState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
        base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }

    public override void OnEnter()
    {
        Debug.Log("RunFallState");
        
        animator.Play("Fall");
    }
    
    public override void Update()
    {
        player.playerPhysicsController.FallModule.BufferJump(inputReader.GetJumpState());
        player.playerPhysicsController.FallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
        
        turnChecker.TurnCheck(inputReader.GetMoveDirection());
    }
    
    private Vector2 _moveVelocity;


    public override void FixedUpdate()
    {
        _moveVelocity.y = player.playerPhysicsController.FallModule.HandleFalling(physicsHandler2D.GetLastAppliedVelocity()).y;
        _moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetLastAppliedVelocity(), inputReader.GetMoveDirection(), playerControllerStats.RunSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        physicsHandler2D.AddVelocity(_moveVelocity);
    }
	
    public override void OnExit()
    {
        
    }
}