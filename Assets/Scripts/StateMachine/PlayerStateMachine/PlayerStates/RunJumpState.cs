using UnityEngine;

public class RunJumpState : BaseState
{
    public RunJumpState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
        base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }
	
    public override void OnEnter()
    {
        Debug.Log("RunJumpState");

        animationController.PlayAnimation("Jump");
    }
    
    public override void Update()
    {
        playerPhysicsController.JumpModule.HandleInput(inputReader.GetJumpState());
        
        turnChecker.TurnCheck(inputReader.GetMoveDirection());
        
        playerPhysicsController.JumpModule.OnMultiJump += () => animationController.PlayAnimation("MultiJump");
    }

    private Vector2 _moveVelocity;

    public override void FixedUpdate()
    {		
        _moveVelocity.y = playerPhysicsController.JumpModule.UpdatePhysics(inputReader.GetJumpState(), physicsHandler2D.GetVelocity()).y;
        _moveVelocity.x = playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.RunSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        // physicsHandler2D.AddVelocity(_moveVelocity);
        
        var gravity = playerPhysicsController.FallModule.ApplyGravity(_moveVelocity, playerControllerStats.Gravity, playerControllerStats.JumpGravityMultiplayer);
        physicsHandler2D.AddVelocity(gravity);
    }

    public override void OnExit()
    {
        playerPhysicsController.JumpModule.OnExitJump();
    }
}