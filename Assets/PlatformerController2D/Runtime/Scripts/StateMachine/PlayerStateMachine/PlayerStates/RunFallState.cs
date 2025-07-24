using UnityEngine;

public class RunFallState : BaseState
{
    private readonly FallModule _fallModule;
    private readonly MovementModule _movementModule;
    
    private Vector2 _moveVelocity;

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
         
        turnChecker.TurnCheck(inputReader.GetMoveDirection());
    }

    public override void FixedUpdate()
    {
        _moveVelocity.y = _fallModule.HandleFalling(physicsHandler2D.GetVelocity()).y;
        
        _moveVelocity.y = _fallModule.HandleFalling(Vector2.zero).y;

        _moveVelocity.x = _movementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetNormalizedHorizontalDirection(), playerControllerStats.RunSpeed, playerControllerStats.RunAirAcceleration, playerControllerStats.RunAirDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        
        
        
        physicsHandler2D.AddVelocity(_moveVelocity);
    }
	
    public override void OnExit()
    {
        
    }
}