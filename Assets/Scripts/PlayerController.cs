using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
	//TODO Реализовать дебаг функции для всех механик: 1. Линия за персонажем 2. Точка на линии когда нажат прыжок
	//TODO Собственная Буферизация Прыжка
	//TODO Траектория для прыжка (Куда прыгать предикт)

	[Header("References")]
	[SerializeField] InputReader input;
	[SerializeField] PlayerControllerStats stats;
	private CollisionsChecker _collisionsChecker;
	private Rigidbody2D _rigidbody;
	private CapsuleCollider2D _capsuleCollider;

	// InputReader parameters
	private Vector2 _moveDirection;

	// Movement parameters
	private Vector2 _moveVelocity;
	private Vector2 targetVelocity;

	//Calculate jump vars
	float AdjustedJumpHeight => stats.maxJumpHeight * stats.jumpHeightCompensationFactor;
	float gravity => 2f * AdjustedJumpHeight / MathF.Pow(stats.timeTillJumpApex, 2f);
	float maxJumpVelocity => gravity * stats.timeTillJumpApex;
	float minJumpVelocity => Mathf.Sqrt(2 * stats.minJumpHeight * gravity);

	private bool _isJumping;
	public float NumberAvailableJumps = 2f;
	private float _numberAvailableJumps;

	private bool _jumpKeyIsPressed; // Нажата
	private bool _jumpKeyWasPressed; // Была нажата
	private bool _jumpKeyWasLetGo; // Была отпущена

	//Debug var
	private float maxYPosition;

	// Timers
	private CountdownTimer _jumpCoyoteTimer;
	private CountdownTimer _jumpBufferTimer;
	public float _bufferTime = 1f;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collisionsChecker = GetComponent<CollisionsChecker>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();

		_jumpCoyoteTimer = new CountdownTimer(stats.CoyoteTime);
		_jumpBufferTimer = new CountdownTimer(_bufferTime);
	}

	private void Update()
	{
		HandleTimers();
		Debbuging();
	}

	private void FixedUpdate()
	{
		HandleMovement();

		// HandleGravity();
		//HandleJump(); //FIXME
		TestJump();


		ApplyMovement();
	}

	private void ApplyMovement()
	{
		_rigidbody.velocity = _moveVelocity;
	}

	private void HandleGravity()
	{
		if (_collisionsChecker.IsGrounded && _moveVelocity.y <= 0f)
		{
			_moveVelocity.y = stats.GroundGravity;
		}
		else
		{
			float gravityForce = gravity;

			_moveVelocity.y = Mathf.MoveTowards(_moveVelocity.y, -stats.MaxFallSpeed, gravityForce * Time.fixedDeltaTime);
			_moveVelocity.y -= gravity * stats.GravityMultiplayer * Time.fixedDeltaTime;
		}
	}

	bool _coyoteUsable;
	bool _bufferUsable;
	bool bufferJump = false;
	private bool _wasGroundedLastFrame = true;

	private void HandleJump()
	{
		if (_collisionsChecker.IsGrounded) // FIXME Вычисление максимальной высоты прыжка
		{
			if (maxYPosition > 0)
			{
				// Debug.Log("Максимальная высота прыжка: " + maxYPosition);
				maxYPosition = 0; // Сбрасываем максимальную высоту для следующего прыжка
			}

			_moveVelocity.y = 0;  // Сбрасываем вертикальную скорость на земле
		}

		if (_collisionsChecker.BumpedHead) // FIXME Удар головой и сразу в низ
		{
			_moveVelocity.y = Mathf.Min(0, _moveVelocity.y);
		}

		if (_jumpKeyWasPressed && _bufferUsable && !_collisionsChecker.IsGrounded && _numberAvailableJumps <= 0f)
		{
			if (_isJumping || !_wasGroundedLastFrame)
			{
				_jumpBufferTimer.Start();
				_bufferUsable = false;
			}
		}

		if (_collisionsChecker.IsGrounded)
		{
			_numberAvailableJumps = NumberAvailableJumps;
			_coyoteUsable = true;
			_bufferUsable = true;
			_wasGroundedLastFrame = true;
			_isJumping = false;
			_jumpCoyoteTimer.Reset();
			_jumpBufferTimer.Reset();
		}
		else if (_wasGroundedLastFrame && !_isJumping && _numberAvailableJumps > 0f)
		{
			if (_coyoteUsable)
			{
				_jumpCoyoteTimer.Start();
				_coyoteUsable = false;
			}

			if (_jumpCoyoteTimer.IsFinished)
			{
				_numberAvailableJumps -= 1f;
				_wasGroundedLastFrame = false;
			}
		}

		if (_numberAvailableJumps <= 0f)
		{
			_jumpKeyWasPressed = false;
		}

		if (_jumpBufferTimer.IsRunning)
		{
			if (_jumpKeyWasLetGo)
			{
				bufferJump = true;
			}

			if (_collisionsChecker.IsGrounded)
			{
				_jumpKeyWasPressed = true;

				if (bufferJump)
				{
					_jumpKeyWasLetGo = true;
					bufferJump = false;
				}
			}
		}

		if (_jumpKeyWasPressed)
		{
			_moveVelocity.y = maxJumpVelocity;

			maxYPosition = transform.position.y;
			_numberAvailableJumps -= 1f;

			_jumpKeyWasPressed = false;
			_isJumping = true;
			_coyoteUsable = false;

			_jumpCoyoteTimer.Stop();
			_jumpBufferTimer.Stop();
		}

		if (_jumpKeyWasLetGo)
		{
			if (_moveVelocity.y > minJumpVelocity)
			{
				_moveVelocity.y = minJumpVelocity;
			}

			_jumpKeyWasLetGo = false;
		}

		if (!_collisionsChecker.IsGrounded && transform.position.y > maxYPosition) // FIXME Обновляем максимальную высоту, если персонаж поднимается
		{
			maxYPosition = transform.position.y;
		}

		_moveVelocity.y -= gravity * stats.GravityMultiplayer * Time.fixedDeltaTime;

		// _moveVelocity.y = Mathf.MoveTowards(_moveVelocity.y, -stats.MaxFallSpeed, gravity * Time.fixedDeltaTime);			
	}

	private bool cutJump;
	private bool cutJumpBuffer;
	private bool isJumping;
	private bool coyoteUsable;
	public float numberJump = 1;

	// private bool jumpBuffer;
	private bool minJumpBuffer = false;


	private void TestJump()
	{
		if (_collisionsChecker.IsGrounded) // TODO Можно отдельно вывести в HandleGravity 
		{
			_moveVelocity.y = -1f;
			numberJump = 1;
			isJumping = false;
			coyoteUsable = true;
		}
		else if (!_collisionsChecker.IsGrounded && !isJumping && coyoteUsable)
		{
			_jumpCoyoteTimer.Reset();
			_jumpCoyoteTimer.Start();
			coyoteUsable = false;
		}

		if (_jumpKeyWasPressed)
		{
			// if (!_collisionsChecker.IsGrounded) { jumpBuffer = true; } //FIXME

			_jumpBufferTimer.Reset();
			_jumpBufferTimer.Start();
		}
		if (_jumpKeyWasLetGo)
		{
			// if (jumpBuffer) { minJumpBuffer = true; } //FIXME
			if (_jumpBufferTimer.IsRunning) { minJumpBuffer = true; }

			cutJump = true;
		}

		// if (_jumpBufferTimer.IsFinished) { jumpBuffer = false; minJumpBuffer = false; }
		if (_jumpBufferTimer.IsFinished) { minJumpBuffer = false; }

		if (_jumpBufferTimer.IsRunning && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning))
		{
			_moveVelocity.y = maxJumpVelocity;

			isJumping = true;

			_jumpBufferTimer.Stop();
			_jumpCoyoteTimer.Stop();

			// jumpBuffer = false;
			if (minJumpBuffer)
			{
				if (_moveVelocity.y > minJumpVelocity)
				{
					_moveVelocity.y = minJumpVelocity;
				}
				minJumpBuffer = false;
			}
		}
		if (cutJump)
		{
			if (_moveVelocity.y > minJumpVelocity)
			{
				_moveVelocity.y = minJumpVelocity;
			}
			cutJump = false;
		}
		
		// if (jumpBuffer && _collisionsChecker.IsGrounded)
		// {
		// 	if (_moveVelocity.y > minJumpVelocity)
		// 	{
		// 		_moveVelocity.y = minJumpVelocity;
		// 	}
		// 	jumpBuffer = false;
		// 	cutJump = true;
		// }
		// if (cutJump)
		// {

		// 	if (_moveVelocity.y > 0f)
		// 	{
		// 		if (_moveVelocity.y > minJumpVelocity)
		// 		{
		// 			_moveVelocity.y = minJumpVelocity;
		// 			cutJump = false;
		// 		}
		// 	}
		// 	if (_moveVelocity.y < 0f && !_collisionsChecker.IsGrounded)
		// 	{

		// 	}

		// }



		// if (_jumpBufferTimer.IsRunning && isJumping && numberJump > 0f)
		// {
		// 	_moveVelocity.y = maxJumpVelocity;
		// 	numberJump -= 1;

		// 	isJumping = true;
		// 	_jumpBufferTimer.Stop();
		// 	_jumpCoyoteTimer.Stop();
		// }
		// if (cutJump && _moveVelocity.y > 0f)
		// {
		// 	if (_moveVelocity.y > minJumpVelocity)
		// 	{
		// 		_moveVelocity.y = minJumpVelocity;
		// 	}
		// 	cutJump = false;	
		// }
		// Debug.Log(cutJump);

		_jumpKeyWasPressed = false;
		_jumpKeyWasLetGo = false;

		_moveVelocity.y -= gravity * stats.GravityMultiplayer * Time.fixedDeltaTime;
	}

	// public float airAcceleration = 0.1f;
	private void HandleMovement()
	{
		targetVelocity = _moveDirection != Vector2.zero
			? new Vector2(_moveDirection.x, 0f) * stats.MoveSpeed
			: Vector2.zero;

		float smoothFactor = _moveDirection != Vector2.zero
			? stats.Acceleration
			: stats.Deceleration;

		// if (_isGrounded)
		// {
		// 	smoothFactor = _moveDirection != Vector2.zero
		// 		? stats.Acceleration
		// 		: stats.Deceleration;
		// }
		// else
		// {
		// 	smoothFactor = airAcceleration;
		// }

		// _moveVelocity.x = Mathf.SmoothDamp(_moveDirection.x, targetVelocity.x, ref velocityXSmoothing, (_isGrounded ? ground : air));

		// MoveToWord fix 
		_moveVelocity.x = Vector2.Lerp(_moveVelocity, targetVelocity, smoothFactor * Time.fixedDeltaTime).x;
		// _rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);

		// Debug.Log(_moveVelocity);
	}

	private void HandleTimers()
	{
		_jumpCoyoteTimer.Tick(Time.deltaTime);
		_jumpBufferTimer.Tick(Time.deltaTime);
	}

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
		if (!_jumpKeyIsPressed && performed)
		{
			_jumpKeyWasPressed = true;
		}

		if (_jumpKeyIsPressed && !performed)
		{
			_jumpKeyWasLetGo = true;
		}

		_jumpKeyIsPressed = performed;
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

