using UnityEngine;

public class RunFallState : BaseState
{
    public RunFallState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
        base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

    public override void OnEnter()
    {
        Debug.Log("RunFallState");
        
        animationController.PlayAnimation("Fall");
    }
    
    public override void Update()
    {
        playerPhysicsController.FallModule.BufferJump(inputReader.GetJumpState());
        playerPhysicsController.FallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
        
        turnChecker.TurnCheck(inputReader.GetMoveDirection());
    }
    
    private Vector2 _moveVelocity;


    public override void FixedUpdate()
    {
        _moveVelocity.y = playerPhysicsController.FallModule.HandleFalling(physicsHandler2D.GetVelocity()).y;
        _moveVelocity.x = playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.RunSpeed, playerControllerStats.runAirAcceleration, playerControllerStats.runAirDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        physicsHandler2D.AddVelocity(_moveVelocity);
    }
	
    public override void OnExit()
    {
        
    }
}