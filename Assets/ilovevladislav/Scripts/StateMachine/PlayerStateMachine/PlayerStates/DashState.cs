using PlatformerController2D.Runtime.Scripts.PhysicalModules.Modules;
using UnityEngine;

namespace PlatformerController2D.Runtime.Scripts.StateMachine.PlayerStateMachine.PlayerState
{
	public class DashState : BaseState
	{
		private readonly DashModule _dashModule;
		private readonly FallModule _fallModule;
	
		// private Vector2 _moveVelocity;

		public DashState(PlayerStateContext context, DashModule dashModule,  FallModule fallModule) :
			base(context)
		{
			_dashModule = dashModule;
			_fallModule = fallModule;
		}

		public override void OnEnter()
		{		
			Debug.Log("DashState");

			animationController.PlayAnimationWithDuration("Dash", playerControllerStats.DashTime, "DashMultiplier");
		
			_dashModule.StartDash(inputReader.GetMoveDirection());
		}

		public override void Update()
		{
			_dashModule.CheckForDirectionChange(inputReader.GetMoveDirection());
		
			turnChecker.TurnCheck(inputReader.GetMoveDirection());
		}

		public override void FixedUpdate()
		{
			var moveVelocity = _dashModule.HandleDash();
		
			physicsHandler2D.AddVelocity(moveVelocity);
		}
	
		public override void OnExit()
		{
			_dashModule.StopDash();
		}
	}
}
