using UnityEngine;

public class RunFallState : BaseState
{
    private readonly FallModule _fallModule;
    private readonly MovementModule _movementModule;

    public RunFallState(PlayerStateContext context, FallModule fallModule, MovementModule movementModule) :
        base(context)
    {
        _fallModule = fallModule;
        _movementModule = movementModule;
    }

    public override void OnEnter()
    {
        Debug.Log("RunFallState");
        
        animationController.PlayAnimation("Fall");
    }
    
    public override void Update()
    {
        _fallModule.BufferJump(inputReader.GetJumpState());
        _fallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
        
        turnChecker.TurnCheck(inputReader.GetMoveDirection());
    }
    
    private Vector2 _moveVelocity;


    public override void FixedUpdate()
    {
        _moveVelocity.y = _fallModule.HandleFalling(physicsHandler2D.GetVelocity()).y;
        _moveVelocity.x = _movementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.RunSpeed, playerControllerStats.runAirAcceleration, playerControllerStats.runAirDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        physicsHandler2D.AddVelocity(_moveVelocity);
    }
	
    public override void OnExit()
    {
        
    }
}