using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.AI;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
	//TODO Реализовать дебаг функции для всех механик: 1. Линия за персонажем 2. Точка на линии когда нажат прыжок
	//TODO Траектория для прыжка (Куда прыгать предикт)
	//TODO Камера Контроллер 
	//TODO Сквиш при прыжке
	//TODO ledge grab climp unity 2d 
	//TODO Углы
	//TODO Debug.Log(_moveDirection == Vector2.zero && Mathf.Abs(_moveVelocity.x) < 0.01f); IDLE State
	//TODO IDLE state add 
	//TODO Постоянное влкючение movestate, то есть movestate = ground, исправить это 
	//TODO Исправить ситуацию при которой при выходе из RunState в IdleState персонаж сразу останаливается


	[Header("References")]
	[SerializeField] InputReader input;
	[SerializeField] PlayerControllerStats stats;
	[SerializeField] Transform spriteTransform;

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
	private CountdownTimer _crouchRollTimer;

	// State Machine Var
	private StateMachine stateMachine;

	// Dash var
	private float _numberAvailableDash;
	private bool _dashKeyIsPressed;
	private Vector2 _dashDirection;

	// Run var
	private bool _runKeyIsPressed;
	private bool _isRunning = false;

	// Crouch var
	private bool _crouchKeyIsPressed;
	private Vector2 normalHeight => _capsuleCollider.size;
	private Vector2 _crouchRollDirection;
	private bool _isSitting = false;

	private bool _crouchRollKeyIsPressed;

	// 
	public TurnChecker TurnChecker;
	private bool IsFacingRight => TurnChecker.IsFacingRight;

	// Test var
	public float runToMoveThreshold = 0.9f;

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
		_crouchRollTimer = new CountdownTimer(stats.CrouchRollTime);

		TurnChecker = new TurnChecker(); // FIXME

		_collisionsChecker.IsSitting = () => _isSitting;
		_collisionsChecker.IsFacingRight = () => TurnChecker.IsFacingRight;

		SetupStateMachine();
	}

	private void SetupStateMachine()
	{
		stateMachine = new StateMachine();

		var idleState = new IdleState(this);
		var locomotionState = new LocomotionState(this);
		var runState = new RunState(this);
		var crouchState = new CrouchState(this);
		var jumpState = new JumpState(this);
		var fallState = new FallState(this);
		var dashState = new DashState(this);
		var crouchRollState = new CrouchRollState(this);
		var wallJumpState = new WallJumpState(this);

		At(locomotionState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(locomotionState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed));
		At(locomotionState, runState, new FuncPredicate(() => _runKeyIsPressed && _collisionsChecker.IsGrounded));
		At(locomotionState, crouchState, new FuncPredicate(() => _crouchKeyIsPressed));
		At(locomotionState, dashState, new FuncPredicate(() => _dashKeyIsPressed));
		At(locomotionState, idleState, new FuncPredicate(() => _moveDirection == Vector2.zero && Mathf.Abs(_moveVelocity.x) < 0.1f));

		At(jumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && ((_moveVelocity.y < 0f && _positiveMoveVelocity) || _isCutJumping)));
		At(jumpState, dashState, new FuncPredicate(() => _dashKeyIsPressed && _numberAvailableDash > 0f));

		At(fallState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed && (_jumpCoyoteTimer.IsRunning || _numberAvailableJumps > 0f)));
		At(fallState, dashState, new FuncPredicate(() => _dashKeyIsPressed && _numberAvailableDash > 0f));
		At(fallState, jumpState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _jumpBufferTimer.IsRunning));
		At(fallState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero)); // FIXME
		At(fallState, crouchState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _crouchKeyIsPressed)); // FIXME
		At(fallState, runState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _runKeyIsPressed)); // FIXME 
		At(fallState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
		At(fallState, wallJumpState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));

		At(wallJumpState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero));
		At(wallJumpState, locomotionState, new FuncPredicate(() => _collisionsChecker.IsGrounded));
		At(wallJumpState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded && !_collisionsChecker.IsTouchingWall));// FIXME jumpState
		At(wallJumpState, dashState, new FuncPredicate(() => _dashKeyIsPressed && CalculateWallDirectionX() != _moveDirection.x));

		// Any(dashState, new FuncPredicate(() => _dashKeyIsPressed && _numberAvailableDash > 0f && !_isSitting)); //FIXME !IsSitting

		At(idleState, dashState, new FuncPredicate(() => _dashKeyIsPressed));
		At(runState, dashState, new FuncPredicate(() => _dashKeyIsPressed));

		At(dashState, idleState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero));
		At(dashState, runState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded && _runKeyIsPressed));
		At(dashState, locomotionState, new FuncPredicate(() => !_dashTimer.IsRunning && _collisionsChecker.IsGrounded));
		At(dashState, fallState, new FuncPredicate(() => !_dashTimer.IsRunning && !_collisionsChecker.IsGrounded));
		At(dashState, wallJumpState, new FuncPredicate(() => _collisionsChecker.IsTouchingWall));
		At(dashState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed && _numberAvailableJumps > 0f));

		// CROUCH STATE
		At(crouchState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(crouchState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed));
		At(crouchState, crouchRollState, new FuncPredicate(() => _crouchRollKeyIsPressed)); //FIXME Переминовать _dashKeyWasPressed _crouchKeyWasPressed
		At(crouchState, runState, new FuncPredicate(() => _runKeyIsPressed && !_crouchKeyIsPressed && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead)); // FIXME Смотреть выше
		At(crouchState, idleState, new FuncPredicate(() => _moveDirection == Vector2.zero && !_crouchKeyIsPressed && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead)); // FIXME Смотреть выше
		At(crouchState, locomotionState, new FuncPredicate(() => !_crouchKeyIsPressed && _collisionsChecker.IsGrounded && !_collisionsChecker.BumpedHead)); // FIXME Смотреть выше

		// At(crouchRollState, crouchState, new FuncPredicate(() => !_crouchRollTimer.IsRunning || !_collisionsChecker.IsGrounded));
		At(crouchRollState, crouchState, new FuncPredicate(() => !_crouchRollTimer.IsRunning));
		At(crouchRollState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(runState, idleState, new FuncPredicate(() => _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero && Mathf.Abs(_moveVelocity.x) < 0.1f));
		At(runState, idleState, new FuncPredicate(() => !_runKeyIsPressed && _collisionsChecker.IsGrounded && _moveDirection == Vector2.zero)); // FIXME
		// At(runState, locomotionState, new FuncPredicate(() => !_runKeyIsPressed && _collisionsChecker.IsGrounded && _moveVelocity.x < stats.RunSpeed - 0.5f)); // FIXME
		// At(runState, locomotionState, new FuncPredicate(() => !_runKeyIsPressed && _collisionsChecker.IsGrounded && _moveVelocity.x < stats.RunSpeed * 0.9f));  // FIXME
		// At(runState, locomotionState, new FuncPredicate(() => !_runKeyIsPressed && _collisionsChecker.IsGrounded && _moveVelocity.x < stats.RunSpeed * runToMoveThreshold)); // FIXME
		At(runState, locomotionState, new FuncPredicate(() => !_runKeyIsPressed && _collisionsChecker.IsGrounded));
		At(runState, fallState, new FuncPredicate(() => !_collisionsChecker.IsGrounded));
		At(runState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed));
		At(runState, crouchState, new FuncPredicate(() => _crouchKeyIsPressed));

		At(idleState, jumpState, new FuncPredicate(() => _jumpKeyWasPressed));
		At(idleState, crouchState, new FuncPredicate(() => _crouchKeyIsPressed));
		At(idleState, runState, new FuncPredicate(() => _moveDirection != Vector2.zero && _runKeyIsPressed));
		At(idleState, locomotionState, new FuncPredicate(() => _moveDirection != Vector2.zero));

		stateMachine.SetState(idleState);
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

		if (!_collisionsChecker.IsGrounded && transform.position.y > maxYPosition) // бновляем максимальную высоту, если персонаж поднимается
		{
			maxYPosition = transform.position.y;
		}

		BumpedHead();
		ApplyMovement();
		JumpKeyReset();
	}

	#region Crouch
	// Метод вызываемый при входе в состояние приседа
	public void OnEnterCrouch()
	{
		SetCrouchState(true);
	}
	// Метод вызываемый при выходе из состояния приседа
	public void OnExitCrouch()
	{
		if (_dashKeyIsPressed) return;

		SetCrouchState(false);
	}

	// Метод который регулирует высоту спрайта и коллайдера в зависиомсти сидит ли персонаж или стоит
	private void SetCrouchState(bool isCrouching)
	{
		_isSitting = isCrouching;
		// Еси персонаж сидит его высота равна высоте приседа, если нет обычной высоте
		var height = isCrouching ? stats.CrouchHeight : normalHeight.x;
		// Еси персонаж сидит его оффсет равен оффсету приседа, если нет то нулю
		var offset = isCrouching ? -stats.CrouchOffset : 0;

		// Настройка коллайдера
		_capsuleCollider.size = new Vector2(_capsuleCollider.size.x, height);
		_capsuleCollider.offset = new Vector2(_capsuleCollider.offset.x, offset);

		// Настройка спрайта
		spriteTransform.localScale = isCrouching ? new Vector2(1f, stats.CrouchHeight) : Vector2.one;
		spriteTransform.localPosition = isCrouching ? new Vector2(spriteTransform.localPosition.x, offset) : Vector2.zero;
	}

	#region CrouchRoll
	// Метод обработки кувырка
	public void CrouchRoll()
	{
		_moveVelocity.x = _crouchRollDirection.x * stats.CrouchRollVelocity;
	}
	// Метод вызываемый при входе в состояние кувырка в приседе
	public void OnEnterCrouchRoll()
	{
		_crouchRollTimer.Start();
		// Сохранение направления кувырка
		_crouchRollDirection = IsFacingRight ? Vector2.right : Vector2.left;
		_crouchRollKeyIsPressed = false;
	}
	// Метод вызываемый при выходе из кувырка в приседе
	public void OnExitCrouchRoll()
	{
		_crouchRollTimer.Stop();
		_crouchRollTimer.Reset();
		// Если кувырок сделан с уступа вернуть высоту коллайдера и спрайта
		if (!_collisionsChecker.IsGrounded)
			SetCrouchState(false);
	}
	#endregion

	#endregion

	// Регион отвечающий за WallSlide/Jump
	#region WallSlideJump

	public void HandleWallInteraction()
	{
		HandleWallSlide();

		if (_jumpKeyWasPressed)
		{
			HandleWallJump();
		}
	}

	// Обработка скольжения по стене WallSlide
	public void HandleWallSlide()
	{
		// Плавное Изменение Y на стене, для скольжение 
		// Lerp зависит - 1. Начальная скорость скольжения (задается в Enter), 2 - макс. скорость скольжения, 3 - Deceleration скольжения
		_moveVelocity.y = Mathf.Lerp(_moveVelocity.y, -stats.WallSlideSpeedMax, stats.WallSlideDeceleration * Time.fixedDeltaTime); //FIXME

		HandleWallJumpTimer();

		// Если ввод обртаный вводу стены, ввод не равен 0, таймер не идет и на стене, запускаем таймер, отвечающий остаток времени на стене
		// Запуск таймера при попытке слезть со стены, помогает выполнить прыжок от стены
		if (_moveDirection.x != CalculateWallDirectionX() && _moveDirection.x != 0f && !_wallJumpTimer.IsRunning && _wasWallSliding)
		{
			_wallJumpTimer.Start();
		}
	}

	// Метод управления таймером
	private void HandleWallJumpTimer()
	{
		// Управление состоянием таймера и скольжением по стене
		if (_wallJumpTimer.IsFinished)
		{
			// Персонаж продолжает скользить по стене, сбрасываем таймер
			if (_moveDirection.x == CalculateWallDirectionX() || _moveDirection.x == 0f)
			{
				_wallJumpTimer.Stop();
				_wallJumpTimer.Reset();
			}
			else // Если персонаж пытается отойти от стены, останавливаем скольжение
			{
				_wasWallSliding = false;
			}
		}
	}

	// Метод вычисляет направление стены по X
	private float CalculateWallDirectionX()
	{
		return IsFacingRight ? 1f : -1f;
	}

	// Применение прыжка со стены WallJump
	public void HandleWallJump()
	{
		// Расчет направление персонажа по X
		float wallDirectionX = CalculateWallDirectionX();

		// Если ввод в сторону стены
		if (_moveDirection.x == wallDirectionX)
		{
			// Прыжок вверх по стене
			_moveVelocity = new Vector2(-wallDirectionX * stats.WallJumpClimb.x, stats.WallJumpClimb.y);
		}
		else if (_moveDirection.x == 0f) // Если ввод равен 0
		{
			// Прыжок от стены
			_moveVelocity = new Vector2(-wallDirectionX * stats.WallJumpOff.x, stats.WallJumpOff.y);
		}
		else // Если ввод сторону от стены, обратную сторону
		{
			// Прыжок в сторону от стены
			_moveVelocity = new Vector2(-wallDirectionX * stats.WallLeap.x, stats.WallLeap.y);
		}
	}

	// Метод вызываемый при входе в состояние wallJump/Slide
	public void OnEnterWallSliding()
	{
		// Начальная скорость скольжения, x = 0 - персонаж падает вниз после окончания скольжения, то есть не сохраняет скорость
		_moveVelocity = new Vector2(0f, -stats.StartVelocityWallSlide);

		// Сброс таймера
		_wallJumpTimer.Reset();

		// Флаг скольжения по стене
		_wasWallSliding = true;

		// Обнуления максКолвоПрыжков, максКолвоРывков
		_numberAvailableJumps = stats.MaxNumberJumps;
		_numberAvailableDash = stats.MaxNumberDash;
	}

	// Метод вызываемый при выходе в состояние wallJump/Slide
	public void OnExitWallSliding()
	{
		// Остановка таймера
		_wallJumpTimer.Stop();
		// Сброс флага скольжения
		_wasWallSliding = false;
	}
	#endregion

	// Регион отвечающий за Jump
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
		// Изменения Y на высоту прыжка
		_moveVelocity.y = maxJumpVelocity;

		// Уменьшение количества доступных прыжков
		_numberAvailableJumps -= 1;
		// Ставим флаг прыжка
		_isJumping = true;

		// Стоп и сброс таймеров
		_jumpBufferTimer.Stop();
		_jumpCoyoteTimer.Stop();
		_jumpCoyoteTimer.Reset();
		_jumpBufferTimer.Reset();
	}

	private void ExecuteVariableJumpHeight()
	{
		// Ставим флаг короткого прыжка
		_isCutJumping = true;

		// Изменения Y на минимальную высоту прыжка
		if (_moveVelocity.y > minJumpVelocity)
		{
			_moveVelocity.y = minJumpVelocity;
		}
	}

	// Метод вызываемый при выходе из состояния прыжка
	public void OnExitJump()
	{
		_positiveMoveVelocity = false;
		_isCutJumping = false;
	}

	#endregion

	// Регион отвечающий за Dash/Рывок
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
		_dashKeyIsPressed = false; // Сброс флага нажатия клавиши
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

	// Регион отвечающий за Movement/Run
	#region Movement/Run

	private float adaptiveIdleThreshold;
	public float thresholdFactor = 0.1f;
	public void HandleMovement()
	{
		// Выход из состояния бега. Так как вход только при переходе в состояние бега, этот метод помогает реализовать логику
		// Того что в состояние бега можно войти только на земле, а выйти в любвое время.
		if (_isRunning) CheckExitRun();

		// if (_isRunning && _moveDirection == Vector2.zero && Mathf.Abs(_moveVelocity.x) < 0.1f)
		// {
		// 	_isRunning = false;
		// }

		float speed = GetCurrentSpeed();
		float acceleration = GetCurrentAcceleration();
		float deceleration = GetCurrentDeceleration();

		// Вычисление вектора направления перемноженного на скорость
		_targetVelocity = _moveDirection != Vector2.zero
			? new Vector2(_moveDirection.x, 0f) * speed
			: Vector2.zero;

		// Вычисление ускорения или замедления игрока в воздухе или на земле
		float smoothFactor = _moveDirection != Vector2.zero
			? acceleration
			: deceleration;

		// Обработка позиции игрока по X
		_moveVelocity.x = Vector2.Lerp(_moveVelocity, _targetVelocity, smoothFactor * Time.fixedDeltaTime).x;

		// adaptiveIdleThreshold = Mathf.Max(0.05f, Mathf.Abs(_targetVelocity.x) * thresholdFactor); // FIXME
	}

	// Метод для получения текущей скорости
	private float GetCurrentSpeed()
	{
		if (_isSitting) return stats.CrouchMoveSpeed;
		if (_isRunning) return stats.RunSpeed;
		return stats.MoveSpeed;
	}
	// Метод для получения текущего ускорения
	private float GetCurrentAcceleration()
	{
		if (!_collisionsChecker.IsGrounded) return stats.airAcceleration;
		if (_isSitting) return stats.CrouchAcceleration;
		if (_isRunning) return stats.RunAcceleration;
		return stats.WalkAcceleration;
	}
	// Метод для получения текущей замедления
	private float GetCurrentDeceleration()
	{
		if (!_collisionsChecker.IsGrounded) return stats.airDeceleration;
		if (_isSitting) return stats.CrouchDeceleration;
		if (_isRunning) return stats.RunDeceleration;
		return stats.WalkDeceleration;
	}

	#region Run

	// Метод вызываемый при входе в состояние бега
	public void OnEnterRun()
	{
		// Нельзя войти в состояние бега находясь не на земле
		if (_collisionsChecker.IsGrounded)
		{
			_isRunning = true;
		}
	}

	// Метод для выхода из бега. Данный метод помогает в реализации механики которая позволяет применять бег только на земле.
	// То есть в воздухе не получится изменить переменную движения на переменную бега. А вот выйти из бега получится.
	private void CheckExitRun()
	{
		if (!_runKeyIsPressed)
		{
			_isRunning = false;
		}
	}

	#endregion

	#endregion

	// Регион отвечающий за Fall/Падение
	#region Fall

	// Метод отвечающий за обработку состояния падения
	public void HandleFalling()
	{
		// Проверка на удар головой об платформу
		// BumpedHead(); //FIXME

		// Запуск таймера прыжка в падении
		if (_jumpKeyWasPressed) { _jumpBufferTimer.Start(); }
		// Сохранение переменной для буферизации минимального прыжка
		if (_jumpBufferTimer.IsRunning && _jumpKeyWasLetGo) { _variableJumpHeight = true; }

		// Применнение гравитации
		// Гравитация в верхней точки прыжыка
		if (Mathf.Abs(_rigidbody.velocity.y) < stats.jumpHangTimeThreshold)
		{
			SetGravity(stats.jumpHangGravityMult);
		} // Гравитация в прыжке (Гравитация если удерживается кнопка прыжка)
		else if (_jumpKeyIsPressed)
		{
			SetGravity(stats.JumpGravityMultiplayer);
		} // Гравитация в падении
		else
		{
			SetGravity(stats.FallGravityMultiplayer);
		}

		// Ограничение максимальной скорости падения
		_moveVelocity.y = Mathf.Clamp(_moveVelocity.y, -stats.maxFallSpeed, 50f);
	}

	// Метод отвечающий за запуск таймера кайота при входе в состояние падения
	public void CoyoteTimerStart()
	{
		// При спуске с платформы запускаем таймер кайота прыжка
		if (_coyoteUsable && !_isJumping) // TODO только на запуске
		{
			_coyoteUsable = false;
			_jumpCoyoteTimer.Start();
		}
	}

	// Метод вызываемый при выходе из состояния падения
	public void OnExitFall() // FIXME 
	{
		// Когда персонаж оказывается на земле, вернуть все флаги, которые обновляются на земле
		// Также используется для коррентной реализации буферного прыжка в StateMachine
		if (_collisionsChecker.IsGrounded) HandleGround();
	}

	#endregion

	#region ReactionToCollisions
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

		_numberAvailableJumps = stats.MaxNumberJumps; // При касании земли возвращение прыжков
		_numberAvailableDash = stats.MaxNumberDash; // При касании земли возвращение рывков

		_isJumping = false; // Сброс флага прыжка
		_coyoteUsable = true; // Установка флага разрешающего делать кайот прыжок

		_moveVelocity.y = stats.GroundGravity; // Гравитация на земле
	}
	#endregion

	#region GeneralMethod

	private void ApplyMovement()
	{
		// Изменение координат игрока игрока
		_rigidbody.velocity = _moveVelocity;
	}

	private void SetGravity(float gravityMulitplayer)
	{
		// Применение гравитации
		_moveVelocity.y -= gravity * gravityMulitplayer * Time.fixedDeltaTime;
	}

	#endregion

	private void JumpKeyReset()
	{
		_jumpKeyWasPressed = false; // Сброс флага нажатой кнопки прыжка
		_jumpKeyWasLetGo = false; // Сброс флага отпущенной кнопки прыжка
	}

	private void HandleTimers() // Сделать список
	{
		_jumpCoyoteTimer.Tick(Time.deltaTime);
		_jumpBufferTimer.Tick(Time.deltaTime);
		_wallJumpTimer.Tick(Time.deltaTime);
		_dashTimer.Tick(Time.deltaTime);
		_crouchRollTimer.Tick(Time.deltaTime);
	}

	#region OnEnableDisable
	void OnEnable()
	{
		input.Move += OnMove;
		input.Jump += OnJump;
		input.Dash += OnDash;
		input.Crouch += OnCrouch;
		input.Run += OnRun;
		input.CrouchRoll += OnCrouchRoll;
	}

	void OnDisable()
	{
		input.Move -= OnMove;
		input.Jump -= OnJump;
		input.Dash -= OnDash;
		input.Crouch -= OnCrouch;
		input.Run -= OnRun;
		input.CrouchRoll -= OnCrouchRoll;
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
		_dashKeyIsPressed = performed;
	}

	private void OnCrouch(bool performed)
	{
		_crouchKeyIsPressed = performed;
	}

	private void OnRun(bool performed)
	{
		_runKeyIsPressed = performed;
	}

	private void OnCrouchRoll(bool performed)
	{
		_crouchRollKeyIsPressed = performed;
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

