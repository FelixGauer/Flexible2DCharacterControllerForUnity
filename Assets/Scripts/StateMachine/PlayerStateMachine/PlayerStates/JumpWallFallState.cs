using UnityEngine;

public class JumpWallFallState : BaseState
{
    private Vector2 _moveVelocity;

    public JumpWallFallState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
        base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

    public override void OnEnter()
    {
        animator.Play("Fall");

        Debug.Log("JumpWallFallState");
    }

    public override void Update()
    {
        player.playerPhysicsController.FallModule.BufferJump(inputReader.GetJumpState());
        player.playerPhysicsController.FallModule.SetHoldState(inputReader.GetJumpState().IsHeld);
		
        turnChecker.TurnCheck(inputReader.GetMoveDirection());
    }

    public override void FixedUpdate()
    {
        _moveVelocity.y = player.playerPhysicsController.FallModule.HandleFalling(physicsHandler2D.GetVelocity()).y;
        _moveVelocity.x = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.wallFallSpeed, playerControllerStats.wallFallAirAcceleration, playerControllerStats.wallFallAirDeceleration).x; // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        physicsHandler2D.AddVelocity(_moveVelocity);
    }
	
    public override void OnExit()
    {
		
    }
}