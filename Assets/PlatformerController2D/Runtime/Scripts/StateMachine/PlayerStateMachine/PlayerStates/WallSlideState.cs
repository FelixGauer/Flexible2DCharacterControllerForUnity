using PlatformerController2D.Runtime.Scripts.PhysicalModules.Modules;
using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.StateMachine.PlayerStateMachine.PlayerState
{
	public class WallSlideState : BaseState
	{
		private readonly WallSlideModule _wallSlideModule;

		public WallSlideState(PlayerStateContext context, WallSlideModule wallSlideModule) : base(context)
		{
			_wallSlideModule = wallSlideModule;
		}

		public override void OnEnter()
		{
			Debug.Log("WallSlideState");
		
			animationController.PlayAnimation("WallSlide");

			_wallSlideModule.OnEnterWallSlide();
		}

		private Vector2 _moveVelocity;

		public override void Update()
		{
			_wallSlideModule.HandleWallDetachmentTimer(inputReader.GetMoveDirection());
		}

		public override void FixedUpdate()
		{
			_moveVelocity = _wallSlideModule.ProcessWallSlide(inputReader.GetMoveDirection());
			physicsHandler2D.AddVelocity(_moveVelocity);
		}

		public override void OnExit()
		{
			_wallSlideModule.OnExitWallSlide();
		}
	}
}