using UnityEngine;

// TODO Будет получать ядро и тем самым ядро будет объявлять какие есть зависимости у конкретной StateMachine

public abstract class BaseState : IState
{
	protected readonly PlayerController player;
	protected readonly Animator animator;
	
	protected BaseState(PlayerController player, Animator animator)
	{
		this.player = player;
		this.animator = animator;
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
		
	}

	public virtual void Update()
	{
	}
}
