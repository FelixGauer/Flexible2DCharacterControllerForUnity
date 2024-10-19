using UnityEngine;

public abstract class BaseState : IState
{
	protected readonly PlayerController player;

	protected BaseState(PlayerController player)
	{
		this.player = player;
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
