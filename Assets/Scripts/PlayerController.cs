using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
	//TODO Реализовать дебаг функции для всех механик: 1. Линия за персонажем 2. Точка на линии когда нажат прыжок
	//TODO Собственная Буферизация Прыжка
	//TODO Создать отдельные скрипты для таймеров
	//TODO Попробовать реализовать прыжок на таймерах
	//TODO Траектория для прыжка (Куда прыгать предикт)

	// private float gravity = -(2f * 6.5f) / MathF.Pow(0.35f, 2f);
	// [Range(0f, 200f)][SerializeField] private float FallAcceleration; // Gravity
	// [Range(0f, 200f)][SerializeField] private float MaxFallSpeed;
	// [Range(0f, 200f)][SerializeField] private float JumpPower;
	// [Range(0f, 200f)][SerializeField] private float JumpEndEarlyGravityModifier;
	// [Range(0f, 2f)][SerializeField] private float JumpBuffer;



	[Header("References")]
	[SerializeField] InputReader input;
	[SerializeField] PlayerControllerStats stats;
	private Rigidbody2D _rigidbody;
	private CapsuleCollider2D _capsuleCollider;
	private BoxCollider2D _feetCollider;


	// InputReader parameters
	private Vector2 _moveDirection;
	private bool _jumpPerformed;


	// CollisionChecks parameters
	private RaycastHit2D _groundHit;
	private RaycastHit2D _headHit;
	private bool _isGrounded;
	private bool _bumbedHead;


	// Movement parameters
	private Vector2 _moveVelocity;
	private Vector2 targetVelocity;


	// Jump parameters
	private bool _jumpToConsume = true;
	private bool _bufferedJumpUsable;
	private bool _endedJumpEarly;
	// private bool _coyoteUsable;
	private float _timeJumpWasPressed;
	private float _time;

	// В прыжке _bufferedJumpUsable = false, _timeJumpWasPressed когда он нажимает время записывается + JumpBuffer(0.2)
	// Сохраняет значение в воздухе когда нажт прыжок и он тру, на земле он ставит _bufferedJumpUsable = true и только тогда применяет
	private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + stats.JumpBuffer;
	// private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
		_feetCollider = GetComponentInChildren<BoxCollider2D>();
	}

	private void Update()
	{
		_time += Time.deltaTime;

		// Debug.Log($"{_time} < {_timeJumpWasPressed + stats.JumpBuffer}");

		// Debug.Log(_moveVelocity);

		Debbuging();
		// Debug.Log(gravity);
		// Debug.Log(_moveVelocity);	
	}

	private void FixedUpdate()
	{
		// if (_time < _timeJumpWasPressed + stats.JumpBuffer)
		// {
		// 	Debug.Log($"{_time} < {_timeJumpWasPressed + stats.JumpBuffer} _bufferedJumpUsable: {_bufferedJumpUsable} ");

		// 	if (_isGrounded)
		// 		Debug.Log(_time);
		// }

		CollisionChecks();
		HandleMovement();

		HandleGravity();
		HandleJump();

		ApplyMovement();
	}

	private void ApplyMovement()
	{
		_rigidbody.velocity = _moveVelocity;
	}

	private void HandleGravity()
	{
		if (_isGrounded && _moveVelocity.y <= 0f)
		{
			_moveVelocity.y = stats.GroundGravity;
		}
		else
		{
			float gravityForce = stats.FallAcceleration;

			// Срочка отвечает за зависимость прыжка от удержания пробела,
			// То как меняется гравитация, от отпускания или удержания пробела
			// Работает только в самый верхней точке, то есть пару кадров
			// if (_endedJumpEarly && _moveVelocity.y > 0) gravityForce *= stats.JumpEndEarlyGravityModifier;

			// Применять fallMultiplayer / JumpEndEarlyGravityModifier каждый кадр
			if ((_endedJumpEarly && _moveVelocity.y > 0) || _moveVelocity.y < 0) gravityForce *= stats.JumpEndEarlyGravityModifier;
			// if (_moveVelocity.y < 0) gravityForce *= stats.JumpEndEarlyGravityModifier; 

			_moveVelocity.y = Mathf.MoveTowards(_moveVelocity.y, -stats.MaxFallSpeed, gravityForce * Time.fixedDeltaTime);

			// Debug.Log(gravityForce);
		}
	}

	private void HandleJump()
	{
		if (!_endedJumpEarly && !_isGrounded && !_jumpPerformed && _moveVelocity.y > 0) _endedJumpEarly = true;
		if (!_jumpToConsume && !HasBufferedJump) return;

		if (_isGrounded) { ExecuteJump(); }

		_jumpToConsume = false;
	}

	private void ExecuteJump()
	{
		_endedJumpEarly = false;
		_timeJumpWasPressed = 0;
		_bufferedJumpUsable = false;
		// _coyoteUsable = false;
		_moveVelocity.y = stats.JumpPower;
	}

	private void HandleMovement()
	{
		targetVelocity = _moveDirection != Vector2.zero
			? new Vector2(_moveDirection.x, 0f) * stats.MoveSpeed
			: Vector2.zero;

		float smoothFactor = _moveDirection != Vector2.zero
			? stats.Acceleration
			: stats.Deceleration;

		// MoveToWord 
		_moveVelocity.x = Vector2.Lerp(_moveVelocity, targetVelocity, smoothFactor * Time.fixedDeltaTime).x;
		// _rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);

		// Debug.Log(_moveVelocity);
	}

	#region CollisionChecks

	private void CollisionChecks()
	{
		IsGrounded();
	}

	private void IsGrounded()
	{
		var bounds = _feetCollider.bounds;
		Vector2 boxCastOrigin = new Vector2(bounds.center.x, bounds.min.y);
		Vector2 boxCastSize = new Vector2(bounds.size.x, stats.GroundDetectionRayLenght);

		_isGrounded = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, stats.GroundDetectionRayLenght, stats.GroundLayer).collider != null;

		if (_isGrounded)
		{
			_bufferedJumpUsable = true; // TODO
		}
	}

	#endregion

	#region OnEnableDisable
	void OnEnable()
	{
		input.Move += OnMove;
		input.Jump += OnJump;
	}

	void OnDisable()
	{
		input.Move -= OnMove;
		input.Jump -= OnJump;
	}
	#endregion

	#region OnMethodActions
	private void OnMove(Vector2 moveDirection)
	{
		_moveDirection = moveDirection;
	}

	private void OnJump(bool performed)
	{
		if (performed)
		{
			// TODO
			_jumpPerformed = true;
			_jumpToConsume = true;
			_timeJumpWasPressed = _time;
		}
		else
			_jumpPerformed = false;

		// Debug.Log(performed);
	}
	#endregion

	//TODO Дебаг для Grounded
	private void Debbuging()
	{
		// Визуализация целевой скорости (Target Velocity)
		Debug.DrawLine(transform.position, transform.position + new Vector3(targetVelocity.x, 0, 0), Color.red);

		// Визуализация текущего вектора скорости (_moveVelocity)
		Debug.DrawLine(transform.position, transform.position + new Vector3(_moveVelocity.x, 0, 0), Color.blue);

		// Визуализация направления движения (_moveDirection)
		if (_moveDirection != Vector2.zero)
		{
			Debug.DrawLine(transform.position, transform.position + new Vector3(_moveDirection.x, 0, 0), Color.green);
		}
		else
		{
			// Визуализация замедления
			if (_moveVelocity != Vector2.zero)
			{
				Debug.DrawLine(transform.position, transform.position + new Vector3(_moveVelocity.x, 0, 0), Color.yellow);
			}
		}
	}


}

