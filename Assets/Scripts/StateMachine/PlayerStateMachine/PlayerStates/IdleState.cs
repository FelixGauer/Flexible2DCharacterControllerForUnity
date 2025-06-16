using UnityEngine;


public class IdleState : BaseState
{
	public IdleState(PlayerController player, Animator animator, InputReader inputReader,
		PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker) { }

	public override void OnEnter()
	{
		Debug.Log("IdleState");

		animator.Play("Idle");

		// player.playerPhysicsController.PhysicsContext.MoveVelocity = Vector2.zero;
		// player.playerPhysicsController.VelocityReset();
		// _physicsHandler2D.ResetVelocity();
		
		player.playerPhysicsController.GroundModule.HandleGround();
	}

	public override void Update()
	{
		// AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		//
		// if (stateInfo.IsName("Land") && stateInfo.normalizedTime >= 1f)
		// {
		// 	// Анимация "Land" полностью проиграна
		// 	animator.Play("Idle");
		// }
		
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{

	}	
}