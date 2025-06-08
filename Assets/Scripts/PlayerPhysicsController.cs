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
	private readonly PlayerControllerStats _stats;
	private readonly TurnChecker _turnChecker;

	private readonly CountdownTimer _jumpCoyoteTimer;
	private readonly CountdownTimer _jumpBufferTimer;
	private readonly CountdownTimer _dashTimer;
	private readonly CountdownTimer _wallJumpTimer;

	private readonly PlayerController _playerController;
	
	public PlayerPhysicsController(
		Rigidbody2D rigidbody,
		CountdownTimer jumpCoyoteTimer,
		CountdownTimer jumpBufferTimer,
		CollisionsChecker collisionsChecker,
		PlayerControllerStats stats,
		PlayerController playerController,
		CountdownTimer dashTimer,
		TurnChecker turnChecker,
		CountdownTimer wallJumpTimer,
		CountdownTimer crouchRollTimer)
	{
		_rigidbody = rigidbody;
		_jumpCoyoteTimer = jumpCoyoteTimer;
		_jumpBufferTimer = jumpBufferTimer;
		_collisionsChecker = collisionsChecker;
		_stats = stats;
		_playerController = playerController;
		_dashTimer = dashTimer;
		_turnChecker = turnChecker;
		_wallJumpTimer = wallJumpTimer;

		PhysicsContext = new PhysicsContext();

		MovementModule = new MovementModule(PhysicsContext);
		GroundModule = new GroundModule(_stats, PhysicsContext);
		JumpModule = new JumpModule(PhysicsContext, _collisionsChecker, _stats, _jumpCoyoteTimer, _jumpBufferTimer);
		FallModule = new FallModule(PhysicsContext, _rigidbody, _collisionsChecker, _stats, _jumpCoyoteTimer, _jumpBufferTimer);
		DashModule = new DashModule(PhysicsContext, _stats, _turnChecker, _dashTimer);
		WallSlideModule = new WallSlideModule(PhysicsContext, _stats, _turnChecker, _wallJumpTimer);
		WallJumpModule = new WallJumpModule(PhysicsContext, _stats, _turnChecker);

		CrouchModule = new CrouchModule(PhysicsContext, _stats, playerController._capsuleCollider, playerController.spriteTransform);
		CrouchRollModule = new CrouchRollModule(PhysicsContext, _stats, crouchRollTimer, turnChecker);

	}

	public void ApplyMovement()
	{
		_rigidbody.linearVelocity = PhysicsContext.MoveVelocity;
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
			Debug.Log("START");
			_jumpCoyoteTimer.Start();
		}
	}
	
	private bool IsFacingRight => _turnChecker.IsFacingRight;
	
	public float CalculateWallDirectionX()
	{
		return IsFacingRight ? 1f : -1f;
	}
}
