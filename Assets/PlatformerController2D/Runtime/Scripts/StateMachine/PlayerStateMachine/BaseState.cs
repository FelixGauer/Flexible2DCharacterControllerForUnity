using PlatformerController2D.Runtime.Scripts.Input;
using PlatformerController2D.Runtime.Scripts.Player;
using PlatformerController2D.Runtime.Scripts.Systems;

// TODO Будет получать ядро и тем самым ядро будет объявлять какие есть зависимости у конкретной StateMachine

namespace PlatformerController2D.Runtime.Scripts.StateMachine.PlayerStateMachine
{
	public abstract class BaseState : IState
	{
		protected readonly PlayerStateContext context;
	
		protected BaseState(PlayerStateContext context)
		{
			this.context = context;
		}

		protected InputReader inputReader => context.InputReader;
		protected PlayerControllerStats playerControllerStats => context.PlayerControllerStats;
		protected PhysicsHandler2D physicsHandler2D => context.PhysicsHandler2D;
		protected TurnChecker turnChecker => context.TurnChecker;
		protected AnimationController animationController => context.AnimationController;
		protected CollisionsChecker collisionsChecker => context.CollisionsChecker;


		public virtual void FixedUpdate()
		{
			//nope
		}

		public virtual void OnEnter()
		{
			//nope
		}

		public virtual void OnExit()
		{
			//nope
		}

		public virtual void Update()
		{
			//nope
		}
	}
}