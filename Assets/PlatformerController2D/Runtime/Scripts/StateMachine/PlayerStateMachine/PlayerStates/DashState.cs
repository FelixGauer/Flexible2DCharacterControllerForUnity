using UnityEngine;


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

		// animationController.PlayAnimation("Dash");
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
		// var gravityVelocity = _fallModule.ApplyGravity(Vector2.zero, playerControllerStats.Gravity, playerControllerStats.DashGravityMultiplayer).y;
		// var gravityVelocity.y = _fallModule.ApplyGravity(Vector2.zero, playerControllerStats.Gravity, playerControllerStats.DashGravityMultiplayer).y;
		// moveVelocity.y = _fallModule.HandleFalling(moveVelocity).y;

		
		physicsHandler2D.AddVelocity(moveVelocity);
		// physicsHandler2D.AddVelocity(new Vector2(0f, gravityVelocity));
	}
	
	public override void OnExit()
	{
		// Сбрасываем горизонтальную скорость
		
		_dashModule.StopDash();
	}
}
