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
	//TODO Создать отдельные скрипты для таймеров
	//TODO Попробовать реализовать прыжок на таймерах
	//TODO Траектория для прыжка (Куда прыгать предикт)
	//TODO GroundChecker отдельный класс 

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

	//FIXME Jump Var 
	[SerializeField] private float jumpDuraction = 0.3f;
	[SerializeField] private float PowerJump = 15f;
	[SerializeField] private float MaxHeightJump = 3f;
	[SerializeField] private float GravityMultiplayer = 1.5f;


	// Timers
	private CountdownTimer _jumpTimer;
	private StopwatchTimer _testTimer;


	//TETS var
	private bool _jumpKeyIsPressed; // Нажата
	private bool _jumpKeyWasPressed; // Была нажата
	private bool _jumpKeyWasLetGo; // Была отпущена

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
		_feetCollider = GetComponentInChildren<BoxCollider2D>();

		// Setup Timers
		_jumpTimer = new CountdownTimer(jumpDuraction); //FIXME Так как в awake не могу менять
		_testTimer = new StopwatchTimer(); //FIXME DELETE
	}

	private void Update()
	{
		HandleTimers();
		HandleJump();
		Debbuging();
	}

	private void FixedUpdate()
	{
		CollisionChecks();
		HandleMovement();

		// HandleGravity();
		Jump(); //FIXME

		ApplyMovement();
	}

	private void ApplyMovement()
	{
		_rigidbody.velocity = _moveVelocity;
	}

	[Header("TEST")]
	public float maxJumpHeight = 5f;
	public float minJumpHeight = 2f;
	public float timeTillJumpApex = 0.35f;
	public float jumpHeightCompensationFactor = 1f;

	// Максимальная высота прыжка (высотка прыжка при однократном нажатии)
	// float AdjustedJumpHeight => jumpHeight * jumpHeightCompensationFactor; 
	float gravity => 2f * maxJumpHeight / MathF.Pow(timeTillJumpApex, 2f);
	float maxJumpVelocity => gravity * timeTillJumpApex;
	float minJumpVelocity => Mathf.Sqrt(2 * minJumpHeight * gravity);

	private void HandleGravity()
	{
		if (_isGrounded && _moveVelocity.y <= 0f)
		{
			_moveVelocity.y = stats.GroundGravity;
		}
		else
		{
			float gravityForce = stats.FallAcceleration;

			// _moveVelocity.y = Mathf.MoveTowards(_moveVelocity.y, -stats.MaxFallSpeed, -grav * Time.fixedDeltaTime);
			_moveVelocity.y = Mathf.MoveTowards(_moveVelocity.y, -stats.MaxFallSpeed, gravityForce * Time.fixedDeltaTime);
			// _moveVelocity.y -= gravityForce * 1.5f *  Time.fixedDeltaTime;
		}
	}

	private void HandleJump()
	{
		if (_jumpPerformed && _isGrounded)
		{
			_jumpTimer.Start();
			// ExecuteJump();
		}
		else if (!_jumpPerformed && _jumpTimer.IsRunning)
		{
			_jumpTimer.Stop();
		}
	}

	// [SerializeField] private float launchPoint = 0.9f;

	private float maxYPosition;
	private float jumpTimer;
	private void Jump()
	{
		// float currentYPosition = transform.position.y;
		// if (currentYPosition > maxYPosition)
		// {
		// 	maxYPosition = currentYPosition;
		// }
		// Debug.Log(maxYPosition); // TODO

		if (_isGrounded)
		{
			if (maxYPosition > 0)
			{
				Debug.Log("Максимальная высота прыжка: " + maxYPosition);
				maxYPosition = 0; // Сбрасываем максимальную высоту для следующего прыжка
			}

			_moveVelocity.y = 0;  // Сбрасываем вертикальную скорость на земле
		}
		
		if (!_isGrounded)
		{
			_jumpKeyWasPressed = false;
		}

		// Начало прыжка
		if (_jumpKeyWasPressed && _isGrounded)
		{
			_moveVelocity.y = maxJumpVelocity;
			maxYPosition = transform.position.y;
		}

		if (_jumpKeyWasLetGo)
		{
			if (_moveVelocity.y > minJumpVelocity)
			{
				_moveVelocity.y = minJumpVelocity;
			}
			_jumpKeyWasLetGo = false;
		}

		if (!_isGrounded && transform.position.y > maxYPosition)
		{
			maxYPosition = transform.position.y;  // Обновляем максимальную высоту, если персонаж поднимается
		}

		_moveVelocity.y -= gravity * GravityMultiplayer * Time.fixedDeltaTime; // stats.FallAcceleration : gravity			

		// if (_jumpKeyWasLetGo)
		// {
		// 	_moveVelocity.y = minJumpVelocity;

		// 	// if (_moveVelocity.y > minJumpVelocity)
		// 	// {
		// 	// 	_moveVelocity.y = minJumpVelocity;
		// 	// }
		// }

		// _moveVelocity.y -= gravity * GravityMultiplayer * Time.fixedDeltaTime; // stats.FallAcceleration : gravity			

		// if (!_jumpTimer.IsRunning && _isGrounded)
		// {
		// 	_jumpTimer.Stop();
		// 	return;
		// }

		// if (_jumpPerformed && _jumpTimer.IsRunning)
		// // if (_jumpPerformed)
		// {
		// 	_testTimer.Start();

		// 	maxYPosition = transform.position.y; 
		// 	_moveVelocity.y = Mathf.Sqrt(2 * AdjustedJumpHeight * gravity); 
		// 	// _moveVelocity.y = initialJumpVelocity;

		// 	// _testTimer.Stop();
		// 	// _jumpPerformed = false;
		// }
		// else 
		// { 
		// 	// Debug.Log(_testTimer.GetTime()); 
		// 	_testTimer.Stop();
		// 	_jumpPerformed = false;

		// 	_moveVelocity.y -= gravity * GravityMultiplayer * Time.fixedDeltaTime; // stats.FallAcceleration : gravity			
		// }


	}

	private void ExecuteJump()
	{
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

		// MoveToWord fix 
		_moveVelocity.x = Vector2.Lerp(_moveVelocity, targetVelocity, smoothFactor * Time.fixedDeltaTime).x;
		// _rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);

		// Debug.Log(_moveVelocity);
	}

	private void HandleTimers()
	{
		_jumpTimer.Tick(Time.deltaTime);
		_testTimer.Tick(Time.deltaTime);
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

		// if (_isGrounded)
		// {
		// 	_bufferedJumpUsable = true; // TODO
		// }
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

