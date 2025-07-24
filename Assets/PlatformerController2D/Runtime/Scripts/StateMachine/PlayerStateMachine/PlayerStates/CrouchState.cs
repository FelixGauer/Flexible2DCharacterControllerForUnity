using UnityEngine;


public class CrouchState : BaseState
{
	private readonly CrouchModule _crouchModule;
	private readonly MovementModule _movementModule;
	
	private Vector2 _moveVelocity;

	public CrouchState(PlayerStateContext context, CrouchModule crouchModule, MovementModule movementModule) :
		base(context)
	{
		_crouchModule = crouchModule;
		_movementModule = movementModule;
	}

	public override void OnEnter()
	{		
		Debug.Log("CrouchState");
		
		animationController.PlayAnimation("CrouchWalk");
		
		_crouchModule.SetCrouchState(true);
	}
	
	public override void Update()
	{
		turnChecker.TurnCheck(inputReader.GetMoveDirection());
		
		float currentSpeed = physicsHandler2D.GetVelocity().magnitude;
		animationController.SyncAnimationWithMovement(currentSpeed, playerControllerStats.BaseCrouchAnimationSpeed);
	}

	public override void FixedUpdate()
	{
		_moveVelocity = _movementModule.HandleMovement(physicsHandler2D.GetVelocity(), inputReader.GetNormalizedHorizontalDirection(), playerControllerStats.CrouchMoveSpeed, playerControllerStats.CrouchAcceleration, playerControllerStats.CrouchDeceleration);
		physicsHandler2D.AddVelocity(_moveVelocity);
	}

	public override void OnExit()
	{
		if (inputReader.GetDashState().WasPressedThisFrame) return;
		
		_crouchModule.SetCrouchState(false);

	}
}
