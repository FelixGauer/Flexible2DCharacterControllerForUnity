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
	
	public readonly PhysicsContext PhysicsContext;

	private readonly Rigidbody2D _rigidbody;
	private readonly CollisionsChecker _collisionsChecker;
	private readonly PlayerControllerStats _playerControllerStats;
	private readonly TurnChecker _turnChecker;

	private readonly PhysicsHandler2D _physicsHandler2D;

	private readonly CountdownTimer _jumpCoyoteTimer;
	private readonly CountdownTimer _jumpBufferTimer;
	private readonly CountdownTimer _dashTimer;
	private readonly CountdownTimer _wallJumpTimer;
	
	public PlayerPhysicsController(
		Rigidbody2D rigidbody,
		CountdownTimer jumpCoyoteTimer,
		CountdownTimer jumpBufferTimer,
		CollisionsChecker collisionsChecker,
		PlayerControllerStats playerControllerStats,
		PlayerController playerController,
		CountdownTimer dashTimer,
		TurnChecker turnChecker,
		CountdownTimer wallJumpTimer,
		CountdownTimer crouchRollTimer,
		PhysicsHandler2D physicsHandler2D)
	{
		_rigidbody = rigidbody;
		_jumpCoyoteTimer = jumpCoyoteTimer;
		_jumpBufferTimer = jumpBufferTimer;
		_collisionsChecker = collisionsChecker;
		_playerControllerStats = playerControllerStats;
		_dashTimer = dashTimer;
		_turnChecker = turnChecker;
		_wallJumpTimer = wallJumpTimer;
		_physicsHandler2D = physicsHandler2D;

		PhysicsContext = new PhysicsContext();

		MovementModule = new MovementModule(PhysicsContext);
		GroundModule = new GroundModule(_playerControllerStats, PhysicsContext, _physicsHandler2D);
		JumpModule = new JumpModule(PhysicsContext, _collisionsChecker, _playerControllerStats, _jumpCoyoteTimer, _jumpBufferTimer);
		FallModule = new FallModule(PhysicsContext, _rigidbody, _collisionsChecker, _playerControllerStats, _jumpCoyoteTimer, _jumpBufferTimer);
		DashModule = new DashModule(PhysicsContext, _playerControllerStats, _turnChecker, _dashTimer);
		WallSlideModule = new WallSlideModule(PhysicsContext, _playerControllerStats, _turnChecker, _wallJumpTimer);
		WallJumpModule = new WallJumpModule(PhysicsContext, _playerControllerStats, _turnChecker);
		CrouchModule = new CrouchModule(PhysicsContext, _playerControllerStats, playerController._capsuleCollider, playerController.spriteTransform);
		CrouchRollModule = new CrouchRollModule(PhysicsContext, _playerControllerStats, crouchRollTimer, turnChecker);
	}
	
	public void ApplyMovement()
	{
		_rigidbody.linearVelocity = PhysicsContext.MoveVelocity;
	}
	
	public void ApplyMovementTestVer1(Vector2 moveVelocity)
	{
		_rigidbody.linearVelocity = moveVelocity;
	}
	
	public void BumpedHead()
	{
		// Проверка не ударился ли персонаж головой платформы
		if (_collisionsChecker.BumpedHead)
		{
			var moveVelocity = PhysicsContext.MoveVelocity;
			// Отправить персонажа вниз
			moveVelocity.y = Mathf.Min(0, moveVelocity.y);
			PhysicsContext.MoveVelocity = moveVelocity;
		}
	}
	
	public void CoyoteTimerStart()
	{
		if (!_collisionsChecker.IsGrounded)
		{
			_jumpCoyoteTimer.Start();
		}
	}
	
	private bool IsFacingRight => _turnChecker.IsFacingRight;
	
	public float CalculateWallDirectionX()
	{
		return IsFacingRight ? 1f : -1f;
	}
	
	public void HandleGround()
	{
		PhysicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
		PhysicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash; // При касании земли возвращение рывков
	}

	// public void VelocityReset()
	// {
	// 	PhysicsContext.MoveVelocity = Vector2.zero;
	// }
}


