using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
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
	private TrailRenderer trailRenderer; //FIXME
	private List<GameObject> markers = new List<GameObject>(); // Список для хранения всех меток

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

		trailRenderer = GetComponent<TrailRenderer>();
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
		HandleGravity();

		ApplyMovement();
		
	}

	private void ApplyMovement()
	{
		_rigidbody.velocity = _moveVelocity;
		// _rigidbody.MovePosition(_rigidbody.position + _moveVelocity * Time.fixedDeltaTime);

	}

	private void OLDHandleGravity()
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

	private bool _variableJumpHeight;
	private bool isJumping;
	private bool coyoteUsable;

	private bool _variableJumpHeightuffer = false;

	private void TestJump()
	{
		// Ударился голвой - опускаем героя вниз
		if (_collisionsChecker.BumpedHead)
		{
			_moveVelocity.y = Mathf.Min(0, _moveVelocity.y);
		}

		// Находится на земле - обнуление переменных
		if (_collisionsChecker.IsGrounded)
		{
			if (!_jumpBufferTimer.IsRunning && _moveDirection != Vector2.zero)
				ClearMarkers();

			_moveVelocity.y = stats.GroundGravity;
			_numberAvailableJumps = NumberAvailableJumps;
			isJumping = false;
			coyoteUsable = true;
		} // Кайот прыжок - запускаем таймер кайота
		else if (!_collisionsChecker.IsGrounded && !isJumping && coyoteUsable)
		{
			// _jumpCoyoteTimer.Reset();
			_jumpCoyoteTimer.Start();
			coyoteUsable = false;
		}

		// Запуск буфера прыжка - проверка на нажатие клавиши
		if (_jumpKeyWasPressed)
		{
			AddJumpMarker();

			// _jumpBufferTimer.Reset();
			_jumpBufferTimer.Start();
		} // Обрезанный прыжок - проверка на отпускание клавиши
		if (_jumpKeyWasLetGo)
		{
			if (_jumpBufferTimer.IsRunning) { _variableJumpHeightuffer = true; }

			_variableJumpHeight = true;
		}

		// Отмена буферизации обрезанного прыжка
		if (_jumpBufferTimer.IsFinished) { _variableJumpHeightuffer = false; }

		// Запуск прыжка и буферизации обрезанного прыжка
		if (_jumpBufferTimer.IsRunning && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning))
		{
			ExecuteJump();

			if (_variableJumpHeightuffer)
			{
				ExecuteVariableJumpHeight();
				_variableJumpHeightuffer = false;
			}
		}
		if (_variableJumpHeight)
		{
			ExecuteVariableJumpHeight();
			_variableJumpHeight = false;
		}

		// Multi/Double JUMP
		if (_jumpBufferTimer.IsRunning && !_collisionsChecker.IsGrounded)
		{
			if (_jumpCoyoteTimer.IsFinished)
			{
				_numberAvailableJumps -= 1f;
			}
			if (_numberAvailableJumps > 0f)
			{
				ExecuteJump();
			}
		}

		// Полное завершение кнопок управления
		_jumpKeyWasPressed = false;
		_jumpKeyWasLetGo = false;

		// Ограничение максимальной скорости падения
		_moveVelocity.y = Mathf.Clamp(_moveVelocity.y, -20f, 50f);
	}

	// Код прыжка
	private void ExecuteJump()
	{
		_numberAvailableJumps -= 1;
		_moveVelocity.y = maxJumpVelocity;

		isJumping = true;
		
		_jumpBufferTimer.Stop();
		_jumpCoyoteTimer.Stop();
		_jumpCoyoteTimer.Reset();
		_jumpBufferTimer.Reset();
	}

	// Код неполного прыжка
	private void ExecuteVariableJumpHeight()
	{
		if (_moveVelocity.y > minJumpVelocity)
		{
			_moveVelocity.y = minJumpVelocity;
		}
	}
	
	private void HandleGravity()
	{
		// Если герой летит вниз повышение гравитации
		if (isJumping && _moveVelocity.y < 0f)
		{
			_moveVelocity.y -= gravity * 1.6f * Time.fixedDeltaTime;
		}
		else // Применнение гравитации
		{
			_moveVelocity.y -= gravity * stats.GravityMultiplayer * Time.fixedDeltaTime;
		}
	}

	// public float airAcceleration = 0.1f;
	// private Vector2 _velocityRef;

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
		_moveVelocity.x = Vector2.Lerp(_moveVelocity, targetVelocity, smoothFactor * Time.fixedDeltaTime).x; //FIXME

		// _moveVelocity = Vector2.SmoothDamp(_moveVelocity, targetVelocity, ref _velocityRef, smoothFactor, Mathf.Infinity, Time.deltaTime);

		// _moveVelocity.x = Mathf.MoveTowards(_moveVelocity.x, targetVelocity.x, smoothFactor * Time.fixedDeltaTime); //FIXME

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

	#region  OnMethodActions
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

	#region Debbug
	public GameObject markerPrefab;

	void AddJumpMarker()
	{
		// Получаем позицию конца линии TrailRenderer
		Vector3 trailEndPosition = GetTrailEndPosition();

		// Спавним метку на этой позиции и сохраняем её в список markers
		GameObject newMarker = Instantiate(markerPrefab, trailEndPosition, Quaternion.identity);
		markers.Add(newMarker); // Добавляем созданную метку в список
	}

	public void ClearMarkers()
	{
		foreach (GameObject marker in markers)
		{
			Destroy(marker);
		}

		markers.Clear();
	}

	Vector3 GetTrailEndPosition()
	{
		// Trail Renderer хранит все точки следа, берем последнюю из них
		Vector3[] positions = new Vector3[trailRenderer.positionCount];
		trailRenderer.GetPositions(positions);

		if (positions.Length > 0)
		{
			// Конец линии - это последняя точка в массиве
			return positions[positions.Length - 1];
		}
		else
		{
			// Если точек нет, возвращаем позицию игрока
			return transform.position;
		}
	}

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
	#endregion
}

