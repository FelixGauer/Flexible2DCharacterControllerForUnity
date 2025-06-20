using System;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

public class PlayerPhysicsController
{
	public readonly MovementModule MovementModule;
	public readonly GroundModule GroundModule;
	public readonly JumpModule JumpModule;
	public readonly FallModule FallModule; 
	public readonly DashModule DashModule;
	public readonly WallSlideModule WallSlideModule;
	public readonly CrouchModule CrouchModule;
	public readonly CrouchRollModule CrouchRollModule;
	public readonly WallJumpModule WallJumpModule;
	
	private readonly Rigidbody2D _rigidbody;
	private readonly CollisionsChecker _collisionsChecker;
	private readonly PlayerControllerStats _playerControllerStats;
	private readonly TurnChecker _turnChecker;

	private readonly PhysicsHandler2D _physicsHandler2D;
	private readonly PlayerTimerRegistry _playerTimerRegistry;

	private readonly CountdownTimer _jumpCoyoteTimer;
	private readonly CountdownTimer _jumpBufferTimer;
	private readonly CountdownTimer _wallJumpTimer;
	
	public PlayerPhysicsController(
		Rigidbody2D rigidbody,
		CollisionsChecker collisionsChecker,
		PlayerControllerStats playerControllerStats,
		PlayerController playerController,
		TurnChecker turnChecker,
		PhysicsHandler2D physicsHandler2D,
		PlayerTimerRegistry playerTimerRegistry,
		ColliderSpriteResizer colliderSpriteResizer)
	{
		_rigidbody = rigidbody;
		_collisionsChecker = collisionsChecker;
		_playerControllerStats = playerControllerStats;
		_turnChecker = turnChecker;
		_physicsHandler2D = physicsHandler2D;
		_playerTimerRegistry = playerTimerRegistry;

		MovementModule = new MovementModule();
		JumpModule = new JumpModule(_collisionsChecker, _playerControllerStats, playerTimerRegistry.jumpBufferTimer, playerTimerRegistry.jumpCoyoteTimer);
		FallModule = new FallModule(_playerControllerStats, playerTimerRegistry.jumpBufferTimer);
		DashModule = new DashModule(_playerControllerStats, _turnChecker, playerTimerRegistry.dashTimer);
		WallSlideModule = new WallSlideModule(_playerControllerStats, _turnChecker, playerTimerRegistry.wallJumpTimer);
		WallJumpModule = new WallJumpModule(_playerControllerStats);
		CrouchModule = new CrouchModule(_playerControllerStats, _collisionsChecker, colliderSpriteResizer);
		CrouchRollModule = new CrouchRollModule(_playerControllerStats, turnChecker, playerTimerRegistry.crouchRollTimer);
		
		_collisionsChecker.OnGroundTouched += HandleGround;
		_collisionsChecker.OnWallTouched += HandleGround;
	}
		
	public void HandleGround()
	{
		// _physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
		// _physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash; // При касании земли возвращение рывков

		JumpModule.ResetNumberAvailableJumps();
		DashModule.ResetNumberAvailableDash();

		// var moveVelocity = _playerControllerStats.GroundGravity; // Гравитация на земле 
        
		// _physicsHandler2D.AddVelocity(new Vector2(0f, moveVelocity));
	}
	
	public Vector2 ApplyGravity(Vector2 moveVelocity, float gravity, float gravityMultiplayer)
	{
		// Применение гравитации
		moveVelocity.y -= gravity * gravityMultiplayer * Time.fixedDeltaTime;
		return moveVelocity;
	}
	

	public void CoyoteTimerStart()
	{
		if (!_collisionsChecker.IsGrounded)
		{
			// _jumpCoyoteTimer.Start();
			_playerTimerRegistry.jumpCoyoteTimer.Start();
		}
	}
	
	public void BumpedHead()
	{
		// Проверка не ударился ли персонаж головой платформы
		// if (_collisionsChecker.BumpedHead)
		// {
		// 	var moveVelocity = PhysicsContext.MoveVelocity;
		// 	// Отправить персонажа вниз
		// 	moveVelocity.y = Mathf.Min(0, moveVelocity.y);
		// 	PhysicsContext.MoveVelocity = moveVelocity;
		// }
		
		if (_collisionsChecker.BumpedHead)
		{
			var moveVelocity = _physicsHandler2D.GetVelocity();
			moveVelocity.y = Mathf.Min(0, moveVelocity.y);
			_physicsHandler2D.AddVelocity(new Vector2(0f, moveVelocity.y));
		}
	}


	private bool IsFacingRight => _turnChecker.IsFacingRight;
	
	public float CalculateWallDirectionX()
	{
		return IsFacingRight ? 1f : -1f;
	}


	public event Func<bool> CanMultiJumpRequested;
	
	public bool CanMultiJump() => CanMultiJumpRequested?.Invoke() ?? false;

}