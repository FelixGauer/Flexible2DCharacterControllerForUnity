using UnityEngine;


public class WallSlideState : BaseState
{
	public WallSlideState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D) :
		base(player, animator, inputReader, playerControllerStats, physicsHandler2D) { }

	public override void OnEnter()
	{
		animator.Play("WallSlide");

		Debug.Log("WallSlideState");

		player.playerPhysicsController.WallSlideModule.OnEnterWallSliding();
		
		// player.playerPhysicsController.MovementModule.ResetMoveVelocity();
	}

	private Vector2 _moveVelocity;

	public override void FixedUpdate()
	{
		_moveVelocity = player.playerPhysicsController.WallSlideModule.HandleWallSlide(physicsHandler2D.GetVelocity(), inputReader.GetMoveDirection());
		physicsHandler2D.AddVelocity(_moveVelocity);
	}

    public override void OnExit()
    {
        player.playerPhysicsController.WallSlideModule.OnExitWallSliding();
    }
}