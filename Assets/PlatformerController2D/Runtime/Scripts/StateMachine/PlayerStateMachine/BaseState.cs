using UnityEngine;

// TODO Будет получать ядро и тем самым ядро будет объявлять какие есть зависимости у конкретной StateMachine

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