using UnityEngine;


public class IdleState : BaseState
{
	public IdleState(PlayerStateContext context) :
		base(context) { }

	public override void OnEnter()
	{
		Debug.Log("IdleState");

		animationController.PlayAnimation("Idle");

		// Debug.Log(turnChecker.IsFacingRight);
	}

	public override void Update()
	{
		// Debug.Log(turnChecker.IsFacingRight);

		turnChecker.TurnCheck(inputReader.GetMoveDirection());
	}

	public override void FixedUpdate()
	{

	}	
}