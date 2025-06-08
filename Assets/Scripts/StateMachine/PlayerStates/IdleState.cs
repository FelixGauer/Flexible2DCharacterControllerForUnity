using UnityEngine;


public class IdleState : BaseState
{
	public IdleState(PlayerController player, Animator animator) : base(player, animator) { }

	public override void OnEnter()
	{
		// animator.Play("Land");

		animator.Play("Idle");

		// player._moveVelocity.x = 0f;
		player.playerPhysicsController.PhysicsContext.MoveVelocity = Vector2.zero;
		
		Debug.Log("IdleState");
		
		// player.playerPhysicsController.HandleGround();
		

		// player.playerPhysicsController._jumpModule.HandleGround();
		
		// player.playerPhysicsController.HandleGround2();
		
		
		// player.HandleGround();

		// player.playerPhysicsController._fallModule.OnExitFall();

		player.playerPhysicsController.GroundModule.HandleGround();
	}



	public override void Update()
	{
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		if (stateInfo.IsName("Land") && stateInfo.normalizedTime >= 1f)
		{
			// Анимация "Land" полностью проиграна
			animator.Play("Idle");
		}
	}

	public override void FixedUpdate()
	{
		// UnityEngine.Debug.Log(player.playerPhysicsController._physicsContext.NumberAvailableJumps);
		// player.SMMove();
		// player.HandleMovement();
	}	
}