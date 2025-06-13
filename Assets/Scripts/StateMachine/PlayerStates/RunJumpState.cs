using UnityEngine;

public class RunJumpState : BaseState
{
    public RunJumpState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
        base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }
	
    public override void OnEnter()
    {
        animator.Play("Jump");

        Debug.Log("RunJumpState");
    }
    
    public override void Update()
    {
        // player.playerPhysicsController.JumpModule.Test1Update(inputReader.GetJumpState());
        
        player.playerPhysicsController.JumpModule.HandleInput(inputReader.GetJumpState());
        
        turnChecker.TurnCheck(inputReader.GetMoveDirection());

    }

    private Vector2 _moveVelocity;

    public override void FixedUpdate()
    {		
        // player.HandleJump();
        // player.HandleMovement();
		
        // player.playerPhysicsController.HandleJump(player._jumpKeyWasPressed, player._jumpKeyWasLetGo);
        // player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.RunSpeed  , player.stats.RunAcceleration, player.stats.RunDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        
        // player.playerPhysicsController.JumpModule.HandleJump(player.input.JumpInputButtonState);

        
        // _moveVelocity.y = player.playerPhysicsController.JumpModule.Test1FixedUpdate(inputReader.GetJumpState(), physicsHandler2D.GetVelocity()).y;
        _moveVelocity.y = player.playerPhysicsController.JumpModule.UpdatePhysics(inputReader.GetJumpState(), physicsHandler2D.GetVelocity()).y;
        _moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.RunSpeed, playerControllerStats.airAcceleration, playerControllerStats.airDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        physicsHandler2D.AddVelocity(_moveVelocity);
    }

    public override void OnExit()
    {
        // player.OnExitJump();

        // player.playerPhysicsController.OnExitJump(); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        
        player.playerPhysicsController.JumpModule.OnExitJump(); // FIXME
    }
}