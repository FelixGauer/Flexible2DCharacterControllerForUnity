using System.Net.NetworkInformation;
using UnityEngine;


public class LocomotionState : BaseState
{
	private readonly InputReader _input;
	private readonly PlayerControllerStats _stats;

	public LocomotionState(PlayerController player, Animator animator, InputReader input, PlayerControllerStats stats) :
		base(player, animator)
	{
		_input = input;
		_stats = stats;
	}

	public override void OnEnter()
	{
		animator.Play("Run");
		
		player.playerPhysicsController.GroundModule.HandleGround();

		Debug.Log("MoveEnter");
	}

	public override void Update()
	{

	}

	public override void FixedUpdate()
	{
		player.playerPhysicsController.MovementModule.HandleMovement(_input.GetMoveDirection(), _stats.MoveSpeed, player.stats.WalkAcceleration, _stats.WalkDeceleration); // player.GetMoveDirection заменить на InputHandler.GetMoveDirection
	}

	public override void OnExit()
	{
		// player.playerPhysicsController.FallModule.CoyoteTimerStart();  // FIXME
		player.playerPhysicsController.CoyoteTimerStart();
	}
}
