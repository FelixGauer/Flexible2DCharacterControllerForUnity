using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
	//TODO Реализовать дебаг функции для всех механик: 1. Линия за персонажем 2. Точка на линии когда нажат прыжок
	//TODO Траектория для прыжка (Куда прыгать предикт)
	//TODO Камера Контроллер 
	//TODO Dash
	//TODO Сквиш при прыжке
	//TODO ledge grab climp unity 2d 
	//TODO Углы
	//TODO WAllSlide DecelSpeed

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
	private Vector2 _targetVelocity;

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
	private bool _isCutJumping = false;
	private bool _coyoteUsable;
	private bool _variableJumpHeight;
	private bool _positiveMoveVelocity = false;

	// WallJump var
	private bool _wasWallSliding = false;

	//Debug var
	private float maxYPosition;
	private TrailRenderer _trailRenderer;
	private List<GameObject> markers = new List<GameObject>();

	// Timers
	private CountdownTimer _jumpCoyoteTimer;
	private CountdownTimer _jumpBufferTimer;
	private CountdownTimer _wallJumpTimer;
	private CountdownTimer _dashTimer;

	// State Machine Var
	private StateMachine stateMachine;

	// Dash var
	private float _numberAvailableDash;
	private bool _dashKeyWasPressed;
	private Vector2 _dashDirection;
	
	// 
	public TurnChecker TurnChecker;
	private bool IsFacingRight => TurnChecker.IsFacingRight;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collisionsChecker = GetComponent<CollisionsChecker>();
		_capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
		_trailRenderer = GetComponent<TrailRenderer>();

		_jumpCoyoteTimer = new CountdownTimer(stats.CoyoteTime);
		_jumpBufferTimer = new CountdownTimer(stats.BufferTime);
		_wallJumpTimer = new CountdownTimer(stats.WallJumpTime);
		_dashTimer = new CountdownTimer(stats.DashTime);

		TurnChecker = new TurnChecker(); // FIXME

		SetupStateMachine();
	}

	private void SetupStateMachine()
	{
		stateMachine = new StateMachine();

		var locomotionState = new LocomotionState(this);
		var jumpState = new JumpState(this);
		var idleState = new IdleState(this);
		var fallState = new FallState(this);
		var wallJumpState = new WallJumpState(this);
		var dashState = new DashState(this);

		At(locomotionState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && _moveVelocity.y < 0f && _positiveMoveVelocity));
		At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && _collisionsChecker.BumpedHead));
		At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && _isCutJumping));
		At(locomotionState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed || _jumpBufferTimer.IsRunning));

		At(fallState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
		At(fallState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed && (_jumpCoyoteTimer.IsRunning || _numberAvailableJumps > 0f)));

		// TO WALLJUMP
		At(fallState, wallJumpState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));

		// FROM WALLJUMP
		At(wallJumpState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));// FIXME
		At(wallJumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));// FIXME jumpState

		// DASH
		// At(locomotionState, dashState, new FuncPredicate(() => _dashKeyWasPressed));
		// At(jumpState, dashState, new FuncPredicate(() => _dashKeyWasPressed));
		// At(fallState, dashState, new FuncPredicate(() => _dashKeyWasPressed));
		Any(dashState, new FuncPredicate(() => _dashKeyWasPressed && _numberAvailableDash > 0f));

		At(dashState, locomotionState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && !_dashKeyWasPressed));
		At(dashState, fallState, new FuncPredicate(() => !_dashTimer.IsRunning && !_collisionsChecker.IsGrounded && !_dashKeyWasPressed));
		At(dashState, wallJumpState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
		At(dashState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed && _numberAvailableJumps > 0f));
		// At(dashState, wallJumpState, new FuncPredicate(() => !_dashTimer.IsRunning && !_collisionsChecker.IsTouchingWall && !_dashKeyWasPressed));

		stateMachine.SetState(locomotionState);
	}

	void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
	void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

	private void Update()
	{
		stateMachine.Update();

		TurnChecker.TurnCheck(_moveDirection, transform, _wasWallSliding); // FIXME

		HandleTimers();
		Debbuging();
	}

	private void FixedUpdate()
	{
		stateMachine.FixedUpdate();
		// Debug.Log(_dashKeyWasPressed);

		if (!_collisionsChecker.IsGrounded && transform.position.y > maxYPosition) // FIXME Обновляем максимальную высоту, если персонаж поднимается
		{
			maxYPosition = transform.position.y;
		}

		// HandleDash();

		ApplyMovement();
		JumpKeyReset();
	}

	#region WallSlide

	public void HandleWallInteraction()
	{
		HandleWallSlide();

		if (_jumpKeyWasPressed)
		{
			HandleWallJump();
		}
	}

	public float WallSlideGravityMultiplayer = 1f;
	public float decelerationWallSlide = 0.1f;
	public float startVelocitySlide = 1f;
	public void HandleWallSlide()
	{
		// _moveVelocity.y = Mathf.Max(_moveVelocity.y, -stats.WallSlideSpeedMax) - gravity * stats.JumpGravityMultiplayer * Time.fixedDeltaTime; // FIXME 	JumpGravityMultiplayer
		// _moveVelocity.y = Mathf.Max(_moveVelocity.y, -stats.WallSlideSpeedMax) - gravity * WallSlideGravityMultiplayer * Time.fixedDeltaTime;

		_moveVelocity.y = Mathf.Lerp(_moveVelocity.y, -stats.WallSlideSpeedMax, decelerationWallSlide * Time.fixedDeltaTime); //FIXME

		float wallDirX = IsFacingRight ? 1f : -1f;

		if (_wallJumpTimer.IsFinished)
		{
			_wasWallSliding = false;
		}

		if (_moveDirection.x != wallDirX && _moveDirection.x != 0f && !_wallJumpTimer.IsRunning && _wasWallSliding)
		{
			_wallJumpTimer.Start();
		}
	}

	public void HandleWallJump()
	{
		float wallDirX = IsFacingRight ? 1f : -1f;

		if (_moveDirection.x == wallDirX)
		{
			// Прыжок с поднятием по стене
			_moveVelocity.x = -wallDirX * stats.WallJumpClimb.x;
			_moveVelocity.y = stats.WallJumpClimb.y;
		}
		else if (_moveDirection.x == 0f)
		{
			// Прыжок с отталкиванием от стены
			_moveVelocity.x = -wallDirX * stats.WallJumpOff.x;
			_moveVelocity.y = stats.WallJumpOff.y;
		}
		else
		{
			// Прыжок в сторону от стены
			_moveVelocity.x = -wallDirX * stats.WallLeap.x;
			_moveVelocity.y = stats.WallLeap.y;
		}
	}

	public void EnterWallSliding()
	{
		// Персонаж после касения стены падает вниз
		// _moveVelocity.x = 0f; // FIXME
		// _moveVelocity.y = -startVelocitySlide;
		_moveVelocity = new Vector2(0f, -startVelocitySlide);

		_wallJumpTimer.Reset();

		_wasWallSliding = true;
		_numberAvailableJumps = stats.MaxNumberJumps;
		_numberAvailableDash = stats.MaxNumberDash;
	}

	public void ExitWallSliding()
	{
		_wallJumpTimer.Stop();

		_wasWallSliding = false;
	}
	#endregion

	#region Jump
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
		SetGravity(stats.JumpGravityMultiplayer);
	}

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

	private void ExecuteVariableJumpHeight()
	{
		_isCutJumping = true;

		if (_moveVelocity.y > minJumpVelocity)
		{
			_moveVelocity.y = minJumpVelocity;
		}
	}

	public void ExitJump()
	{
		_positiveMoveVelocity = false;
		_isCutJumping = false;
	}

	#endregion

	// Регион отвечающий за DASH/РЫВОК
	#region Dash

	// Обработка состояния рывка
	public void HandleDash()
	{
		// Изменение скорости по оси X для совершение рывка
		_moveVelocity.x = _dashDirection.x * stats.DashVelocity;

		// Если скорость по y не равна 0, применяем рывок в оси Y
		if (_dashDirection.y != 0)
		{
			_moveVelocity.y = _dashDirection.y * stats.DashVelocity;
		}

		// Отмена рывка, если игрок проживаем противоположное направление
		if (_dashDirection == -_moveDirection)
		{
			OnExitDash();
		}

		// Применение гравитации во время рывка
		SetGravity(stats.DashGravityMultiplayer);
	}

	// Метод вызываемый при входе в состояние рывка
	public void OnEnterDash()
	{
		_dashTimer.Start(); // Запуск таймера рывка
		_dashKeyWasPressed = false; // Сброс флага нажатия клавиши
		_numberAvailableDash -= 1; // Уменьшение количество оставшихся рывков
		_moveVelocity.y = 0f; // Сброс скорости по Y, для расчета правильного направления рывка
	}

	// Метод вызываемый при выходе из состояния рывка
	public void OnExitDash()
	{
		_dashTimer.Stop(); // Остановка таймера
		_dashTimer.Reset(); // Сброс таймера
	}
	
	// Расчет направления рывка
	public void CalculateDashDirection()
	{
		_dashDirection = input.Direction;
		_dashDirection = GetClosestDirection(_dashDirection); // Поиск ближайшего допустимого направления
	}

	// Метод для поиска ближайшего направления рывка
	private Vector2 GetClosestDirection(Vector2 targetDirection)
	{
		Vector2 closestDirection = Vector2.zero; // Начальное значение для ближайшего направления
		float minDistance = float.MaxValue;      // Минимальная дистанция для поиска ближайшего направления

		// Перебор всех допустимых направления в общем массиве направлений
		foreach (var dashDirection in stats.DashDirections)
		{
			float distance = Vector2.Distance(targetDirection, dashDirection);
			
			// Проверка на диагональное направление
			if (IsDiagonal(dashDirection))
			{
				distance = 1f;
			}
			// Если найдено близкое направление, обновляем ближайшее и минимальную дистанцию
			if (distance < minDistance)
			{
				minDistance = distance;
				closestDirection = dashDirection;
			}
		}

		// Если стоит на месте, применяем рывок в сторону поворота игрока, иначе в найденое ближайшее направление
		return closestDirection == Vector2.zero ? (IsFacingRight ? Vector2.right : Vector2.left) : closestDirection;
	}

	// Проверка является ли направление диагональным
	private bool IsDiagonal(Vector2 direction)
	{
		return Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1;
	}

	#endregion

	public void HandleMovement()
	{
		_targetVelocity = _moveDirection != Vector2.zero
			? new Vector2(_moveDirection.x, 0f) * stats.MoveSpeed
			: Vector2.zero;

		float smoothFactor = _moveDirection != Vector2.zero
			? (_collisionsChecker.IsGrounded ? stats.Acceleration : stats.airAcceleration)
			: (_collisionsChecker.IsGrounded ? stats.Deceleration : stats.airDeceleration);

		_moveVelocity.x = Vector2.Lerp(_moveVelocity, _targetVelocity, smoothFactor * Time.fixedDeltaTime).x; //FIXME
																											  // Debug.Log((Mathf.Abs(_moveVelocity.x) > 0.01f));

		// _moveVelocity.x = Mathf.SmoothDamp(_moveDirection.x, _targetVelocity.x, ref velocityXSmoothing, (_isGrounded ? ground : air));

		// MoveToWord fix 

		// _moveVelocity = Vector2.SmoothDamp(_moveVelocity, _targetVelocity, ref _velocityRef, smoothFactor, Mathf.Infinity, Time.deltaTime);

		// _moveVelocity.x = Mathf.MoveTowards(_moveVelocity.x, _targetVelocity.x, smoothFactor * Time.fixedDeltaTime); //FIXME

		// _rigidbody.velocity = new Vector2(_moveVelocity.x, _rigidbody.velocity.y);

		// Debug.Log(_moveVelocity);
	}

	public void HandleFalling()
	{
		BumpedHead(); //FIXME

		// При спуске с платформы запускаем таймер кайота прыжка
		if (_coyoteUsable && !_isJumping) // TODO только на запуске
		{
			_coyoteUsable = false;
			_jumpCoyoteTimer.Start();
		}

		// Запуск таймера прыжка в падении
		if (_jumpKeyWasPressed) { _jumpBufferTimer.Start(); }
		// Сохранение переменной для буферизации минимального прыжка
		if (_jumpBufferTimer.IsRunning && _jumpKeyWasLetGo) { _variableJumpHeight = true; }

		// _positiveMoveVelocity = false;
		// _isCutJumping = false;

		if (Mathf.Abs(_rigidbody.velocity.y) < stats.jumpHangTimeThreshold)
		{
			SetGravity(stats.jumpHangGravityMult);
		}
		else if (_jumpKeyIsPressed)
		{
			SetGravity(stats.JumpGravityMultiplayer);
		}
		else
		{
			SetGravity(stats.FallGravityMultiplayer);
		}

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

	public void HandleGround()
	{
		// Debug.Log(maxYPosition);
		if (maxYPosition > 0)
		{
			maxYPosition = 0;
		}

		_numberAvailableJumps = stats.MaxNumberJumps;
		_numberAvailableDash = stats.MaxNumberDash;

		_isJumping = false;
		_coyoteUsable = true;

		_moveVelocity.y = stats.GroundGravity;
	}

	private void ApplyMovement()
	{
		_rigidbody.velocity = _moveVelocity;
	}

	private void SetGravity(float gravityMulitplayer)
	{
		_moveVelocity.y -= gravity * gravityMulitplayer * Time.fixedDeltaTime;
	}

	private void JumpKeyReset()
	{
		_jumpKeyWasPressed = false;
		_jumpKeyWasLetGo = false;
	}

	private void HandleTimers()
	{
		_jumpCoyoteTimer.Tick(Time.deltaTime);
		_jumpBufferTimer.Tick(Time.deltaTime);
		_wallJumpTimer.Tick(Time.deltaTime);
		_dashTimer.Tick(Time.deltaTime);
	}

	#region OnEnableDisable
	void OnEnable()
	{
		input.Move += OnMove;
		input.Jump += OnJump;
		input.Dash += OnDash;
	}

	void OnDisable()
	{
		input.Move -= OnMove;
		input.Jump -= OnJump;
		input.Dash -= OnDash;
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

	private void OnDash(bool performed)
	{
		_dashKeyWasPressed = performed;
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
		Debug.DrawLine(transform.position, transform.position + new Vector3(_targetVelocity.x, 0, 0), Color.red);

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

