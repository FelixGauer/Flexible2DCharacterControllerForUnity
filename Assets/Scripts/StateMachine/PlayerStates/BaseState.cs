using UnityEngine;

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
		// Debug.Log("----BaseState.OnExit----");
	}

	public virtual void Update()
	{
	}
}
