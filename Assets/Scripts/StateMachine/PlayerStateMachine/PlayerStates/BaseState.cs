using UnityEngine;

// TODO Будет получать ядро и тем самым ядро будет объявлять какие есть зависимости у конкретной StateMachine

public abstract class BaseState : IState
{
	protected readonly PlayerController player;
	protected readonly Animator animator;
	protected readonly PlayerControllerStats playerControllerStats;
	protected readonly InputReader inputReader;
	protected readonly PhysicsHandler2D physicsHandler2D;
	protected readonly TurnChecker turnChecker;
	protected readonly AnimationController animationController;

	
	protected BaseState(PlayerController player, Animator animator, InputReader inputReader, PlayerControllerStats playerControllerStats, PhysicsHandler2D physicsHandler2D, TurnChecker turnChecker, AnimationController animationController)
	{
		this.player = player;
		this.animator = animator;
		this.playerControllerStats = playerControllerStats;
		this.inputReader = inputReader;
		this.physicsHandler2D = physicsHandler2D;
		this.turnChecker = turnChecker;
		this.animationController = animationController;
	}


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
