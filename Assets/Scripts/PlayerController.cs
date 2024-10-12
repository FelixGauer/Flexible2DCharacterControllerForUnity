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
	private CollisionsChecker _collisionsChecker;
	private Rigidbody2D _rigidbody;
	private CapsuleCollider2D _capsuleCollider;
	private BoxCollider2D _feetCollider;

	// InputReader parameters
	private Vector2 _moveDirection;
	private bool _jumpPerformed;

	// CollisionChecks parameters
	private bool _bumbedHead;

	// Movement parameters
	private Vector2 _moveVelocity;
	private Vector2 targetVelocity;

	//FIXME Jump Var 
	[SerializeField] private float jumpDuraction = 0.3f;
	[SerializeField] private float PowerJump = 15f;
	[SerializeField] private float MaxHeightJump = 3f;
	[SerializeField] private float GravityMultiplayer = 1.5f;
	[SerializeField] private float CoyoteTime = 1.5f;

	// Timers
	private CountdownTimer _jumpTimer;
	private CountdownTimer _jumpCoyoteTimer;


	//TETS var
	private bool _jumpKeyIsPressed; // Нажата
	private bool _jumpKeyWasPressed; // Была нажата
	private bool _jumpKeyWasLetGo; // Была отпущена

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
		_collisionsChecker = GetComponent<CollisionsChecker>();
		_feetCollider = GetComponentInChildren<BoxCollider2D>();

		// Setup Timers
		_jumpTimer = new CountdownTimer(jumpDuraction); //FIXME Так как в awake не могу менять
		_jumpCoyoteTimer = new CountdownTimer(CoyoteTime);
	}

	private void Update()
	{
		HandleTimers();
		HandleJump();
		Debbuging();
		
		Debug.Log(_collisionsChecker.IsGrounded);
	}

	private void FixedUpdate()
	{
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

	float AdjustedJumpHeight => maxJumpHeight * jumpHeightCompensationFactor;
	float gravity => 2f * AdjustedJumpHeight / MathF.Pow(timeTillJumpApex, 2f);
	float maxJumpVelocity => gravity * timeTillJumpApex;
	float minJumpVelocity => Mathf.Sqrt(2 * minJumpHeight * gravity);

	private void HandleGravity()
	{
		if (_collisionsChecker.IsGrounded && _moveVelocity.y <= 0f)
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
		if (_jumpPerformed && _collisionsChecker.IsGrounded)
		{
			_jumpTimer.Start();
			// ExecuteJump();
		}
		else if (!_jumpPerformed && _jumpTimer.IsRunning)
		{
			_jumpTimer.Stop();
		}
	}

	private float maxYPosition;
	private float coyoteTimeCounter;
	private void Jump()
	{
		// if (_isGrounded)
		// {
		// 	coyoteTimeCounter = CoyoteTime;
		// }
		// else if (!_isGrounded && !_jumpKeyWasPressed)
		// {
		// 	coyoteTimeCounter -= Time.deltaTime;
		// }
		
		if (_collisionsChecker.IsGrounded) // FIXME Вычисление максимальной высоты прыжка
		{
			if (maxYPosition > 0)
			{
				Debug.Log("Максимальная высота прыжка: " + maxYPosition);
				maxYPosition = 0; // Сбрасываем максимальную высоту для следующего прыжка
			}

			_moveVelocity.y = 0;  // Сбрасываем вертикальную скорость на земле
		}

		// if (_jumpKeyWasPressed && coyoteTimeCounter > 0f)
		if (_jumpKeyWasPressed && _collisionsChecker.IsGrounded)
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
			// coyoteTimeCounter = 0f;

		}
		
		_jumpKeyWasPressed = false;
		_jumpKeyWasLetGo = false;
		
		if (!_collisionsChecker.IsGrounded && transform.position.y > maxYPosition) // FIXME Обновляем максимальную высоту, если персонаж поднимается
		{
			maxYPosition = transform.position.y;  
		}

		_moveVelocity.y -= gravity * GravityMultiplayer * Time.fixedDeltaTime; // stats.FallAcceleration : gravity			
	}

	private void ExecuteJump()
	{
		_moveVelocity.y = stats.JumpPower;
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
		_jumpTimer.Tick(Time.deltaTime);
		_jumpCoyoteTimer.Tick(Time.deltaTime);
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

