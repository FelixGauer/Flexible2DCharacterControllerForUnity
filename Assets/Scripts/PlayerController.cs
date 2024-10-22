using System;
using System.Collections.Generic;
using UnityEngine;


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

	private float _numberAvailableJumps;

	private bool _jumpKeyIsPressed; // Нажата
	private bool _jumpKeyWasPressed; // Была нажата
	private bool _jumpKeyWasLetGo; // Была отпущена

	private bool _isJumping = false;
	private bool _coyoteUsable;
	private bool _variableJumpHeight;
	private bool _positiveMoveVelocity = false;

	//Debug var
	private float maxYPosition;
	private TrailRenderer _trailRenderer;
	private List<GameObject> markers = new List<GameObject>();

	// Timers
	private CountdownTimer _jumpCoyoteTimer;
	private CountdownTimer _jumpBufferTimer;

	// State Machine Var
	private StateMachine stateMachine;

	public bool _isFacingRight { get; private set; }

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collisionsChecker = GetComponent<CollisionsChecker>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
		_trailRenderer = GetComponent<TrailRenderer>();

		_jumpCoyoteTimer = new CountdownTimer(stats.CoyoteTime);
		_jumpBufferTimer = new CountdownTimer(stats.BufferTime);

		_isFacingRight = true;

		SetupStateMachine();
	}

	private void SetupStateMachine()
	{
		stateMachine = new StateMachine();

		var locomotionState = new LocomotionState(this);
		var jumpState = new JumpState(this);
		var idleState = new IdleState(this);
		var fallState = new FallState(this);
		var wallJumpState = new WallJumpState(this); // FIXME

		At(locomotionState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && _moveVelocity.y < 0f && _positiveMoveVelocity));
		At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && _collisionsChecker.BumpedHead));
		At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && _jumpKeyWasLetGo)); //FIXME 

		At(locomotionState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed || _jumpBufferTimer.IsRunning));

		At(fallState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
		At(fallState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed && (_jumpCoyoteTimer.IsRunning || _numberAvailableJumps > 0f))); //FIXME

		// At(jumpState, wallJumpState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall)); // FIXME
		// At(fallState, wallJumpState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));// FIXME
		// At(wallJumpState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));// FIXME
		// At(wallJumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall ));// FIXME

		stateMachine.SetState(locomotionState);
	}

	void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
	void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

	private void Update()
	{
		stateMachine.Update();

		TurnCheck(_moveDirection);

		HandleTimers();
		Debbuging();
	}

	private void FixedUpdate()
	{
		// HandleWallSlide();

		stateMachine.FixedUpdate();

		if (!_collisionsChecker.IsGrounded && transform.position.y > maxYPosition) // FIXME Обновляем максимальную высоту, если персонаж поднимается
		{
			maxYPosition = transform.position.y;
		}

		ApplyMovement();
		JumpKeyReset();

	}
	
	public float _wallSlideSpeedMax = -5f;
	private bool _wasWallSliding = false;
	public Vector2 wallJumpClimb;

	public void HandleWallSlide() // FIXME 
	{
		if (_collisionsChecker.IsGrounded) { _wasWallSliding = false; }

		if (_collisionsChecker.IsTouchingWall)
		{
			_wasWallSliding = true;
			if (_moveVelocity.y < _wallSlideSpeedMax)
			{
				_moveVelocity.y = _wallSlideSpeedMax;
			}
		}
		_moveVelocity.y -= gravity * stats.JumpGravityMultiplayer * Time.fixedDeltaTime; // FIXME

		if (_jumpKeyWasPressed)
		{
			if (_wasWallSliding)
			{
				if (_moveDirection.x == 1f)
				{
					_moveVelocity.x = -1f * wallJumpClimb.x;
					_moveVelocity.y = wallJumpClimb.y;
				}
				else if (_moveDirection.x == -1f)
				{
					_moveVelocity.x = 1f * wallJumpClimb.x;
					_moveVelocity.y = wallJumpClimb.y;
				}
			}
		}
	}

	private void ApplyMovement()
	{
		_rigidbody.velocity = _moveVelocity;
	}

	public void HandleJump()
	{
		// Проверка на забуферизированный минимальный прыжок (пробел для буфера сразу прожат и отпущен)
		if (_jumpBufferTimer.IsFinished) { _variableJumpHeight = false; }

		// Запуск буфера прыжка при нажатии пробела
		if (_jumpKeyWasPressed) { _jumpBufferTimer.Start(); }

		// Проверка для прыжка кайота и мултипрыжка, нужна для того чтобы контроллировать переход в состояние падения
		if (_moveVelocity.y > 0f) { _positiveMoveVelocity = true; }

		// Проверка на возможность прыжка (обычный прыжок или мультипрыжок)
		if (_jumpBufferTimer.IsRunning && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning || _numberAvailableJumps > 0f))
		{
			// Если не на земле и таймер кайота завершён — вычитаем прыжок
			if (!_collisionsChecker.IsGrounded && _jumpCoyoteTimer.IsFinished)
			{
				_numberAvailableJumps -= 1f;
			}

			ExecuteJump();

			// Запуск минимального забуферированного прыжка (если пробел был быстро отпущен)
			if (_variableJumpHeight)
			{
				ExecuteVariableJumpHeight();
				_variableJumpHeight = false;
			}
		}

		// Контроль высоты прыжка в зависимости от удержания кнопки прыжка
		if (_jumpKeyWasLetGo)
		{
			ExecuteVariableJumpHeight();
		}

		// Применение гравитации в прыжке до состояния падения
		_moveVelocity.y -= gravity * stats.JumpGravityMultiplayer * Time.fixedDeltaTime; // FIXME
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
		if (_jumpKeyWasPressed) { _jumpBufferTimer.Start(); }
		// Сохранение переменной для буферизации минимального прыжка
		if (_jumpBufferTimer.IsRunning && _jumpKeyWasLetGo) { _variableJumpHeight = true; }

		_positiveMoveVelocity = false;
		// Примененеие гравитации падения
		// _moveVelocity.y -= gravity * stats.FallGravityMultiplayer * Time.fixedDeltaTime;

		if (_jumpKeyIsPressed) //FIXME
			_moveVelocity.y -= gravity * stats.JumpGravityMultiplayer * Time.fixedDeltaTime; // FIXME
		else
			_moveVelocity.y -= gravity * stats.FallGravityMultiplayer * Time.fixedDeltaTime;

		// Ограничение максимальной скорости падения
		_moveVelocity.y = Mathf.Clamp(_moveVelocity.y, -stats.maxFallSpeed, 50f);
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

		_numberAvailableJumps = stats.numberAvailableJumps;

		_isJumping = false;
		_coyoteUsable = true;

		_moveVelocity.y = stats.GroundGravity;
	}

	private void JumpKeyReset()
	{
		_jumpKeyWasPressed = false;
		_jumpKeyWasLetGo = false;
	}

	// Код прыжка
	private void ExecuteJump()
	{
		_moveVelocity.y = maxJumpVelocity;

		_numberAvailableJumps -= 1;
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

	// private Vector2 _velocityRef;

	public void HandleMovement()
	{
		targetVelocity = _moveDirection != Vector2.zero
			? new Vector2(_moveDirection.x, 0f) * stats.MoveSpeed
			: Vector2.zero;

		float smoothFactor = _moveDirection != Vector2.zero
			? (_collisionsChecker.IsGrounded ? stats.Acceleration : stats.airAcceleration)
			: stats.Deceleration;

		_moveVelocity.x = Vector2.Lerp(_moveVelocity, targetVelocity, smoothFactor * Time.fixedDeltaTime).x; //FIXME


		// float smoothFactor = _moveDirection != Vector2.zero
		// 	? stats.Acceleration
		// 	: stats.Deceleration;

		// float smoothFactor;
		// if (_collisionsChecker.IsGrounded)
		// {
		// 	smoothFactor = _moveDirection != Vector2.zero
		// 		? stats.Acceleration
		// 		: stats.Deceleration;
		// }
		// else
		// {
		// 	smoothFactor = _moveDirection != Vector2.zero
		// 		? stats.airAcceleration
		// 		: stats.Deceleration;
		// 	// smoothFactor = airAcceleration;
		// }


		// _moveVelocity.x = Mathf.SmoothDamp(_moveDirection.x, targetVelocity.x, ref velocityXSmoothing, (_isGrounded ? ground : air));

		// MoveToWord fix 

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

	//FIXME
	#region TurnCheck
	private void TurnCheck(Vector2 moveDirection)
	{
		if ((moveDirection.x < 0 && _isFacingRight) || (moveDirection.x > 0 && !_isFacingRight))
		{
			Turn();
		}
	}

	private void Turn()
	{
		_isFacingRight = !_isFacingRight;
		transform.Rotate(0f, 180f, 0f);
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
		Vector3[] positions = new Vector3[_trailRenderer.positionCount];
		_trailRenderer.GetPositions(positions);

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

