using System.Linq;
using UnityEngine;


public class CrouchRollState : BaseState
{
	private Vector2 _moveVelocity;
	
	private CrouchRollModule _crouchRollModule;
	private CrouchModule _crouchModule;

	public CrouchRollState(PlayerStateContext context, CrouchRollModule crouchRollModule, CrouchModule crouchModule) : base(context)
	{
		_crouchRollModule = crouchRollModule;
		_crouchModule = crouchModule;
	}
	
	public override void OnEnter()
	{		
		Debug.Log("CrouchRollState");
		
		animationController.PlayAnimationWithDuration(
			"CrouchRoll", 
			playerControllerStats.CrouchRollTime,
			"CrouchRollSpeedMultiplier"
		);
		
		_crouchRollModule.StartCrouchRoll();
	}
	
	public override void Update()
	{
	}


	public override void FixedUpdate()
	{
		_moveVelocity.x = _crouchRollModule.CrouchRoll(physicsHandler2D.GetVelocity()).x;

		physicsHandler2D.AddVelocity(_moveVelocity);
	}

	public override void OnExit()
	{
		animationController.ResetSpeedParameter("CrouchRollSpeedMultiplier");
		
		_crouchModule.SetCrouchState(false);

		_crouchRollModule.StopCrouchRoll();
	}

}
