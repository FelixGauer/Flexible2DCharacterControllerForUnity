using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Packages.Rider.Editor.UnitTesting;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.TextCore.Text;

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

	public float NumberAvailableJumps = 2f;
	private float _numberAvailableJumps;

	private bool _jumpKeyIsPressed; // Нажата
	private bool _jumpKeyWasPressed; // Была нажата
	private bool _jumpKeyWasLetGo; // Была отпущена

	private bool _isJumping = false;
	private bool _coyoteUsable;
	private bool _variableJumpHeight;
	private bool _variableJumpHeightBuffer = false;
	private bool _negativeMoveVeolicty = false;

	//Debug var
	private float maxYPosition;
	private TrailRenderer trailRenderer; //FIXME
	private List<GameObject> markers = new List<GameObject>(); // Список для хранения всех меток

	// Timers
	private CountdownTimer _jumpCoyoteTimer;
	private CountdownTimer _jumpBufferTimer;
	// public float _bufferTime = 1f;

	// State Machine Var
	private StateMachine stateMachine;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collisionsChecker = GetComponent<CollisionsChecker>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();

		_jumpCoyoteTimer = new CountdownTimer(stats.CoyoteTime);
		_jumpBufferTimer = new CountdownTimer(stats.BufferTime);

		trailRenderer = GetComponent<TrailRenderer>();

		//StateMachine

		stateMachine = new StateMachine();

		var locomotionState = new LocomotionState(this);
		var jumpState = new JumpState(this);
		var idleState = new IdleState(this);
		var fallState = new FallState(this);

		At(locomotionState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && _moveVelocity.y < 0f && _negativeMoveVeolicty));
		At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && _collisionsChecker.BumpedHead));

		At(locomotionState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed || _jumpBufferTimer.IsRunning));

		At(fallState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
		At(fallState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed && (_jumpCoyoteTimer.IsRunning || _numberAvailableJumps > 0f)));

		stateMachine.SetState(locomotionState);
	}

	void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
	void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

	private void Update()
	{
		stateMachine.Update();

		HandleTimers();
		Debbuging();
	}

	private void FixedUpdate()
	{
		stateMachine.FixedUpdate();

		if (!_collisionsChecker.IsGrounded && transform.position.y > maxYPosition) // FIXME Обновляем максимальную высоту, если персонаж поднимается
		{
			maxYPosition = transform.position.y;
		}

		// HandleMovement();
		// HandleJump();
		// HandleGravity();
		ApplyMovement();

		JumpKeyReset();
	}

	private void ApplyMovement()
	{
		_rigidbody.velocity = _moveVelocity;
	}

	public void JumpForState()
	{
		// Проверка на забуферизированный минимальный прыжок (пробел для буфера сразу прожат и отпущен)
		if (_jumpBufferTimer.IsFinished) { _variableJumpHeight = false; }

		// Запуск буфера прыжка при нажатии пробела
		if (_jumpKeyWasPressed) { _jumpBufferTimer.Start(); }

		// Проверка для прыжка кайота и мултипрыжка, нужна для того чтобы контроллировать переход в состояние падения
		// При падении с уступа скорость становится отрицатательной и за 1 фрейм она не выходит в положительуню 
		// следовательно скорость будет прыгать от отрицательной к положительной и состояния меняться пока _moveVelocity.y > 0f
		if (_moveVelocity.y > 0f)
		{
			_negativeMoveVeolicty = true;
		}

		// Запуск буф, обычного, кайфот прыжков
		if (_jumpBufferTimer.IsRunning && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning))
		{
			ExecuteJump();
			// Запуск  минимальный забуфериннего прыжка (пробел для буфера сразу прожат и отпущен)
			if (_variableJumpHeight)
			{
				ExecuteVariableJumpHeight();
				_variableJumpHeight = false;
			}
		} 
		// Контролль высоты прыжка в зависимости от удержания кнопки прыжка
		if (_jumpKeyWasLetGo)
		{
			ExecuteVariableJumpHeight();
		}
		// Запуск мультипрыжка
		if (_jumpBufferTimer.IsRunning && !_collisionsChecker.IsGrounded)
		{
			// Проверка если таймер кайота прошел нужно вычесть 1 прыжок 
			if (_jumpCoyoteTimer.IsFinished)
			{
				_numberAvailableJumps -= 1f;
			}
			// Прыгать если количество прыжков больше 0
			if (_numberAvailableJumps > 0f)
			{
				ExecuteJump();
			}
		}
		
		// Примененеи гравитации в прыжке, до состояния падения
		_moveVelocity.y -= gravity * stats.GravityMultiplayer * Time.fixedDeltaTime; // FIXME
	}

	public void FallForState()
	{
		// При спуске с платформы запускаем таймер кайота прыжка
		if (_coyoteUsable && !_isJumping)
		{
			_coyoteUsable = false;
			_jumpCoyoteTimer.Start();
		}

		// Запуск таймера прыжка в падении
		if (_jumpKeyWasPressed) //FIXME
			_jumpBufferTimer.Start();
		// Сохранение переменной для буферизации минимального прыжка
		if (_jumpBufferTimer.IsRunning && _jumpKeyWasLetGo)
			_variableJumpHeight = true;

		_negativeMoveVeolicty = false;
		// Примененеие гравитации падения
		_moveVelocity.y -= gravity * stats.GravityMultiplayer * Time.fixedDeltaTime;

		// Ограничение максимальной скорости падения
		_moveVelocity.y = Mathf.Clamp(_moveVelocity.y, -20f, 50f);
	}

	public void BumpedHead()
	{
		// Проверка не ударился ли персонаж голвоой платформы
		if (_collisionsChecker.BumpedHead)
		{
			// Отправить персонажа вниз
			_moveVelocity.y = Mathf.Min(0, _moveVelocity.y);
		}
	}

	public void GroundedState()
	{
		Debug.Log(maxYPosition);
		if (maxYPosition > 0)
		{
			maxYPosition = 0;
		}
		
		_numberAvailableJumps = NumberAvailableJumps;

		_isJumping = false;
		_coyoteUsable = true;

		_moveVelocity.y = stats.GroundGravity;
	}

	private void JumpKeyReset() //FIXME
	{
		_jumpKeyWasPressed = false;
		_jumpKeyWasLetGo = false;
	}


	public void HandleJump()
	{
		// Ударился головой - опускаем героя вниз
		if (_collisionsChecker.BumpedHead)
		{
			_moveVelocity.y = Mathf.Min(0, _moveVelocity.y);
		}

		if (_collisionsChecker.IsGrounded)
		{
			if (!_jumpBufferTimer.IsRunning && _moveDirection != Vector2.zero)
				ClearMarkers();

			_moveVelocity.y = stats.GroundGravity;
			_numberAvailableJumps = NumberAvailableJumps;
			_isJumping = false;
			_coyoteUsable = true;
		}
		// Кайот прыжок - запускаем таймер кайота
		if (!_collisionsChecker.IsGrounded && !_isJumping && _coyoteUsable)
		{
			// _jumpCoyoteTimer.Reset();
			_jumpCoyoteTimer.Start();
			_coyoteUsable = false;
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
			if (_jumpBufferTimer.IsRunning) { _variableJumpHeightBuffer = true; }

			_variableJumpHeight = true;
		}

		// Отмена буферизации обрезанного прыжка
		if (_jumpBufferTimer.IsFinished) { _variableJumpHeightBuffer = false; }

		// Запуск прыжка и буферизации обрезанного прыжка
		if (_jumpBufferTimer.IsRunning && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning))
		{
			ExecuteJump();

			if (_variableJumpHeightBuffer)
			{
				ExecuteVariableJumpHeight();
				_variableJumpHeightBuffer = false;
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

		if (_isJumping && _moveVelocity.y < 0f)
		{
			_moveVelocity.y -= gravity * 1.6f * Time.fixedDeltaTime;
		}
		else // Применнение гравитации
		{
			_moveVelocity.y -= gravity * stats.GravityMultiplayer * Time.fixedDeltaTime;
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

		_isJumping = true;

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

	// public float airAcceleration = 0.1f;
	// private Vector2 _velocityRef;

	public void HandleMovement()
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

