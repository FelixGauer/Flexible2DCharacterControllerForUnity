using UnityEngine;

public class RunJumpState : BaseState
{
    private readonly JumpModule _jumpModule;
    private readonly FallModule _fallModule;
    private readonly MovementModule _movementModule;
    
    private Vector2 _moveVelocity;

    public RunJumpState(PlayerStateContext context, JumpModule jumpModule, FallModule fallModule, MovementModule movementModule) :
        base(context)
    {
        _jumpModule = jumpModule;
        _fallModule = fallModule;
        _movementModule = movementModule;
    }
	
    public override void OnEnter()
    {
        Debug.Log("RunJumpState");

        animationController.PlayAnimation("Jump");
        _jumpModule.OnMultiJump += () => animationController.PlayAnimation("MultiJump");

        // _jumpModule.StartJump(inputReader.GetJumpState()); 
        _jumpModule.TestStartJump(); 
        
        _jumpModule.SetPlayerTransform(animationController.test); // FIXME
        
        // Debug.Log(physicsHandler2D.GetVelocity());
    }
    
    public override void Update()
    {
        // _jumpModule.HandleInput(inputReader.GetJumpState());
        _jumpModule.HandleInput(inputReader.GetJumpState());
        
        turnChecker.TurnCheck(inputReader.GetMoveDirection());
        
        _fallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
    }


    public override void FixedUpdate()
    {
        // _moveVelocity.y = _jumpModule.UpdatePhysics(inputReader.GetJumpState(), physicsHandler2D.GetVelocity()).y;
        _moveVelocity.y = _jumpModule.JumpPhysicsProcessing(physicsHandler2D.GetVelocity()).y;

        _moveVelocity.x = _movementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetNormalizedHorizontalDirection(), playerControllerStats.RunSpeed, playerControllerStats.AirAcceleration, playerControllerStats.AirDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        
        _moveVelocity.y = _fallModule.HandleFalling(_moveVelocity).y;


        // Debug.Log(_moveVelocity.y);

        physicsHandler2D.AddVelocity(_moveVelocity);
    }

    public override void OnExit()
    {
        _jumpModule.OnExitJump();
        // _jumpModule.ExitJump();
    }
}