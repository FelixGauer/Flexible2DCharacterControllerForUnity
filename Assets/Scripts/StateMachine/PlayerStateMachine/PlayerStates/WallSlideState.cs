using UnityEngine;


public class WallSlideState : BaseState
{
	public WallSlideState(PlayerPhysicsController playerPhysicsController, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(playerPhysicsController, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

	public override void OnEnter()
	{
		Debug.Log("WallSlideState");
		
		animationController.PlayAnimation("WallSlide");

		playerPhysicsController.WallSlideModule.OnEnterWallSlide();
	}

	private Vector2 _moveVelocity;

	public override void FixedUpdate()
	{
		_moveVelocity = playerPhysicsController.WallSlideModule.ProcessWallSlide(inputReader.GetMoveDirection());
		physicsHandler2D.AddVelocity(_moveVelocity);
	}

    public override void OnExit()
    {
        playerPhysicsController.WallSlideModule.OnExitWallSlide();
    }
}