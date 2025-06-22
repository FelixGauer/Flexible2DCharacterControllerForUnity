using UnityEngine;


public class WallSlideState : BaseState
{
	public WallSlideState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D, turnChecker, animationController) { }

	public override void OnEnter()
	{
		animator.Play("WallSlide");

		Debug.Log("WallSlideState");

		player.playerPhysicsController.WallSlideModule.OnEnterWallSlide();
	}

	private Vector2 _moveVelocity;

	public override void FixedUpdate()
	{
		_moveVelocity = player.playerPhysicsController.WallSlideModule.ProcessWallSlide(inputReader.GetMoveDirection());
		physicsHandler2D.AddVelocity(_moveVelocity);
	}

    public override void OnExit()
    {
        player.playerPhysicsController.WallSlideModule.OnExitWallSlide();
    }
}