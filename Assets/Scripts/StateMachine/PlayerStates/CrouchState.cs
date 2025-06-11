using UnityEngine;


public class CrouchState : BaseState
{
	public CrouchState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D) { }

	public override void OnEnter()
	{		
		Debug.Log("CrouchState");
		
		animator.Play("CrouchWalk");
		
		player.playerPhysicsController.CrouchModule.SetCrouchState(true);
		
		player.playerPhysicsController.GroundModule.HandleGround();
	}

	private Vector2 _moveVelocity;

	public override void FixedUpdate()
	{
		_moveVelocity = player.playerPhysicsController.MovementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection(), playerControllerStats.CrouchMoveSpeed, playerControllerStats.CrouchAcceleration, playerControllerStats.CrouchDeceleration);
		physicsHandler2D.AddVelocity(_moveVelocity);
	}

	public override void OnExit()
	{
		if (inputReader.GetDashState().WasPressedThisFrame) return;
		
		player.playerPhysicsController.CrouchModule.SetCrouchState(false);

		// player.OnExitCrouch();
		// player.playerPhysicsController.CrouchModule.OnExitCrouch(player.input.DashInputButtonState);
	}
}
