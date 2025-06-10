using UnityEngine;

public class JumpState : BaseState
{
	public JumpState(PlayerController player, Animator animator) : base(player, animator) { }
	
	public override void OnEnter()
	{
		animator.Play("Jump");
		
		Debug.Log("JumpEnter");
	}

	public override void Update()
	{
		if (player.input.GetJumpState().WasPressedThisFrame && player.playerPhysicsController.PhysicsContext.NumberAvailableJumps == 1)
			animator.Play("MultiJump");
		
		player.playerPhysicsController.JumpModule.Test1Update(player.input.GetJumpState());
	}

	public override void FixedUpdate()
	{
		player.playerPhysicsController.JumpModule.Test1FixedUpdate(player.input.GetJumpState());
		player.playerPhysicsController.MovementModule.HandleMovement(player.input.GetMoveDirection(), player.stats.MoveSpeed, player.stats.airAcceleration, player.stats.airDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
	}

    public override void OnExit()
    {
        player.playerPhysicsController.JumpModule.OnExitJump();

    }
	
}