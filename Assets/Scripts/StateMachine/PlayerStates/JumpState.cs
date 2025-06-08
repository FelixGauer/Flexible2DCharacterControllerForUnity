using UnityEngine;

public class JumpState : BaseState
{
	public JumpState(PlayerController player, Animator animator) : base(player, animator) { }
	
	public override void OnEnter()
	{
		animator.Play("Jump");
		
		Debug.Log("JumpEnter");
		// player.playerPhysicsController.JumpModule.HandleJump(player.input.JumpInputButtonState);
	}

	public override void Update()
	{
		player.playerPhysicsController.JumpModule.Test1Update(player.input.JumpInputButtonState);
		
		if (player.input.JumpInputButtonState.WasPressedThisFrame && player.playerPhysicsController.PhysicsContext.NumberAvailableJumps == 1)
			animator.Play("MultiJump");
	}

	public override void FixedUpdate()
	{
		// player.HandleJump();
		// player.HandleMovement();
		
		// player.playerPhysicsController.HandleJump(player._jumpKeyWasPressed, player._jumpKeyWasLetGo);
		// player.playerPhysicsController.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed  , player.stats.WalkAcceleration, player.stats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
		
		// player.playerPhysicsController.JumpModule.HandleJump(player.input.JumpInputButtonState);
		player.playerPhysicsController.JumpModule.Test2FixedUpdate(player.input.JumpInputButtonState);
		player.playerPhysicsController.MovementModule.HandleMovement(player.GetMoveDirection(), player.stats.MoveSpeed, player.stats.airAcceleration, player.stats.airDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
	}

    public override void OnExit()
    {
        // player.OnExitJump();

        // player.playerPhysicsController.OnExitJump(); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
        
        player.playerPhysicsController.JumpModule.OnExitJump();

    }
	
}