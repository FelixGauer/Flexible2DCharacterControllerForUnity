using UnityEngine;


public class IdleState : BaseState
{
	public IdleState(PlayerStateContext context) :
		base(context) { }

	public override void OnEnter()
	{
		Debug.Log("IdleState");

		animationController.PlayAnimation("Idle");
	}

	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{
		var moveVelocity = new Vector2(0f, playerControllerStats.GroundGravity);
		physicsHandler2D.AddVelocity(moveVelocity);
	}	
}