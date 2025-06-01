using System;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

public class GroundModule
{
	private readonly Rigidbody2D _rigidbody;

	private Vector2 _moveVelocity;
	private readonly PlayerControllerStats _playerControllerStats;
	private readonly PhysicsContext _physicsContext;

	public GroundModule(Rigidbody2D rigidbody, PlayerControllerStats playerControllerStats , PhysicsContext physicsContext) 
	{
		_rigidbody = rigidbody;
		_playerControllerStats = playerControllerStats;
		_physicsContext = physicsContext;
	}

	public void HandleGround()
	{
		// Debug.Log(maxYPosition);
		// if (maxYPosition > 0)
		// {
		// 	maxYPosition = 0;
		// }

		_physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
		_physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash; // При касании земли возвращение рывков

		// _isJumping = false; // Сброс флага прыжка
		// _coyoteUsable = true; // Установка флага разрешающего делать кайот прыжок

		_moveVelocity.y = _playerControllerStats.GroundGravity; // Гравитация на земле 

		_physicsContext.MoveVelocity = _moveVelocity;
	}
}
public class PhysicsContext 
{
	public Vector2 MoveVelocity { get; set; }
	public bool VariableJumpHeight { get; set; }

	public float NumberAvailableJumps { get; set; }
	public float NumberAvailableDash { get; set; }
	
	public Vector2 ApplyGravity(Vector2 moveVelocity, float gravity, float gravityMultiplayer)
	{
		// Применение гравитации
		 moveVelocity.y -= gravity * gravityMultiplayer * Time.fixedDeltaTime;
		 return moveVelocity;
	}
}
public class JumpModule
{
	public JumpModule(PhysicsContext physicsContext, Rigidbody2D rigidbody, CollisionsChecker collisionsChecker, PlayerControllerStats playerControllerStats, CountdownTimer jumpCoyoteTimer, CountdownTimer jumpBufferTimer)
	{
		_rigidbody = rigidbody;
		_collisionsChecker = collisionsChecker;
		_playerControllerStats = playerControllerStats;

		_jumpCoyoteTimer = jumpCoyoteTimer;
		_jumpBufferTimer = jumpBufferTimer;

		_physicsContext = physicsContext;

		// _jumpCoyoteTimer = new CountdownTimer(playerControllerStats.CoyoteTime);
		// _jumpBufferTimer = new CountdownTimer(playerControllerStats.BufferTime);
	}

	private PhysicsContext _physicsContext;
	
	private Rigidbody2D _rigidbody;
	private readonly CollisionsChecker _collisionsChecker;
	private readonly PlayerControllerStats _playerControllerStats;

	private CountdownTimer _jumpCoyoteTimer;
	private CountdownTimer _jumpBufferTimer;
	
	private Vector2 _moveVelocity;
	
	private bool _variableJumpHeight;
	private bool _positiveMoveVelocity;
	private float _numberAvailableJumps;

	private bool _isJumping;
	private bool _isCutJumping;

	public void HandleJump(InputButtonState jumpState)
	{
		_moveVelocity = _physicsContext.MoveVelocity;
		
		// Проверка на забуферизированный минимальный прыжок (пробел для буфера сразу прожат и отпущен)
		if (_jumpBufferTimer.IsFinished) { _physicsContext.VariableJumpHeight = false; }

		// Запуск буфера прыжка при нажатии пробела
		// if (_jumpKeyWasPressed) { _jumpBufferTimer.Start(); }

		// Проверка для прыжка кайота и мултипрыжка, нужна для того чтобы контроллировать переход в состояние падения // для прыжка кайта без этой переменнйо он сразу переходит в фалл
		if (_moveVelocity.y > 0f) { _positiveMoveVelocity = true; }

		// Проверка на возможность прыжка (обычный прыжок или мультипрыжок)
		// if (_jumpBufferTimer.IsRunning && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning || _physicsContext.NumberAvailableJumps > 0f)) 
		if (_jumpBufferTimer.IsRunning || (jumpState.WasPressedThisFrame && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning || _physicsContext.NumberAvailableJumps > 0f)))
		{
			// Если не на земле и таймер кайота завершён — вычитаем прыжок
			if (!_collisionsChecker.IsGrounded && _jumpCoyoteTimer.IsFinished)
			{
				_physicsContext.NumberAvailableJumps -= 1f;
			}
			
			ExecuteJump();
			
			// Запуск укороченного буферного прыжка
			if (_jumpBufferTimer.IsRunning && !jumpState.IsHeld) 
				ExecuteVariableJumpHeight();

			// Уменьшение количества доступных прыжков
			_physicsContext.NumberAvailableJumps -= 1;
			// Ставим флаг прыжка
			_isJumping = true;

			// Стоп и сброс таймеров
			_jumpBufferTimer.Stop();
			_jumpCoyoteTimer.Stop();
			_jumpCoyoteTimer.Reset();
			_jumpBufferTimer.Reset();
			// Запуск минимального забуферированного прыжка (если пробел был быстро отпущен)
			// if (_physicsContext.VariableJumpHeight)
			// {
			// 	ExecuteVariableJumpHeight();
			// 	_physicsContext.VariableJumpHeight = false;
			// }

			// if (_jumpBufferTimer.IsRunning && _jumpKeyWasLetGo)
			// {
			// 	ExecuteVariableJumpHeight();
			// 	Debug.Log("ASD");
			// }
			
			// if (_jumpBufferTimer.IsRunning && _jumpKeyWasLetGo)
			// {
			// 	ExecuteVariableJumpHeight();
			// 	_physicsContext.VariableJumpHeight = false;
			// }
		}

		// Контроль высоты прыжка в зависимости от удержания кнопки прыжка
		if (jumpState.WasReleasedThisFrame)
		{
			ExecuteVariableJumpHeight();
		}

		// Применение гравитации в прыжке до состояния падения		
		// SetGravity(_playerControllerStats.JumpGravityMultiplayer);
		_moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.JumpGravityMultiplayer);

		_physicsContext.MoveVelocity = _moveVelocity;

		// ApplyMovement();
	}
	
	// Метод вызываемый при выходе из состояния прыжка
	public void OnExitJump()
	{
		_positiveMoveVelocity = false;
		_isCutJumping = false;
	}
	
	public bool CanFall()
	{
		// return (_moveVelocity.y < 0f && (_positiveMoveVelocity || _isCutJumping));
		// return ((_moveVelocity.y < 0f && _positiveMoveVelocity) || _isCutJumping);

		return (_moveVelocity.y < 0f && _positiveMoveVelocity);
	}
	
	// Метод для выполнения прыжка
	private void ExecuteJump()
	{
		// Изменения Y на высоту прыжка
		_moveVelocity.y = _playerControllerStats.MaxJumpVelocity;
	}
	
	// Метод для выполнения неполного прыжка
	private void ExecuteVariableJumpHeight()
	{
		// Ставим флаг короткого прыжка
		_isCutJumping = true;

		// Изменения Y на минимальную высоту прыжка
		if (_moveVelocity.y > _playerControllerStats.MinJumpVelocity)
		{
			_moveVelocity.y = _playerControllerStats.MinJumpVelocity;
		}
	}
}
public class FallModule 
{
	public FallModule(PhysicsContext physicsContext, Rigidbody2D rigidbody, CollisionsChecker collisionsChecker, PlayerControllerStats playerControllerStats, CountdownTimer jumpCoyoteTimer, CountdownTimer jumpBufferTimer)
	{
		_rigidbody = rigidbody;
		_collisionsChecker = collisionsChecker;
		_playerControllerStats = playerControllerStats;
		
		_jumpCoyoteTimer = jumpCoyoteTimer;
		_jumpBufferTimer = jumpBufferTimer;

		_physicsContext = physicsContext;
	}
	
	private Rigidbody2D _rigidbody;
	private readonly CollisionsChecker _collisionsChecker;
	private readonly PlayerControllerStats _playerControllerStats;
	
	private CountdownTimer _jumpCoyoteTimer;
	private CountdownTimer _jumpBufferTimer;

	private PhysicsContext _physicsContext;

	private Vector2 _moveVelocity;
	
	public void HandleFalling(InputButtonState jumpState)
	{
		_moveVelocity = _physicsContext.MoveVelocity;
		// Проверка на удар головой об платформу
		// BumpedHead(); //FIXME

		// Запуск таймера прыжка в падении
		if (jumpState.WasPressedThisFrame) { _jumpBufferTimer.Start(); }
		// Сохранение переменной для буферизации минимального прыжка
		if (_jumpBufferTimer.IsRunning && jumpState.WasReleasedThisFrame) { _physicsContext.VariableJumpHeight = true; } //FIXME

		// Применнение гравитации
		// Гравитация в верхней точки прыжыка
		if (Mathf.Abs(_rigidbody.linearVelocity.y) < _playerControllerStats.jumpHangTimeThreshold)
		{
			// SetGravity(_playerControllerStats.jumpHangGravityMult);
			_moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.jumpHangGravityMult);

		} // Гравитация в прыжке (Гравитация если удерживается кнопка прыжка)
		else if (jumpState.IsHeld)
		{
			// SetGravity(_playerControllerStats.JumpGravityMultiplayer);
			_moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.JumpGravityMultiplayer);

		} // Гравитация в падении
		else
		{
			// SetGravity(_playerControllerStats.FallGravityMultiplayer);
			_moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.FallGravityMultiplayer);
		}

		// Ограничение максимальной скорости падения
		_moveVelocity.y = Mathf.Clamp(_moveVelocity.y, -_playerControllerStats.maxFallSpeed, 50f);
		
		_physicsContext.MoveVelocity = _moveVelocity;
		
		// ApplyMovement();
	}

	private bool _coyoteUsable;

	// Метод отвечающий за запуск таймера кайота при входе в состояние падения
	public void CoyoteTimerStart()
	{
		// При спуске с платформы запускаем таймер кайота прыжка
		// if (_coyoteUsable && !_isJumping) // TODO только на запуске 
		if (_coyoteUsable) // TODO только на запуске 
		{
			_coyoteUsable = false;
			_jumpCoyoteTimer.Start();
		}
	}

	// Метод вызываемый при выходе из состояния падения
	public void OnExitFall() // FIXME 
	{
		// Когда персонаж оказывается на земле, вернуть все флаги, которые обновляются на земле
		// Также используется для конкретной реализации буферного прыжка в StateMachine
		if (_collisionsChecker.IsGrounded) HandleGround();
		// if (_collisionsChecker.IsTouchingWall) _isRunning = false; // FIXME
	}
	
	
	public void HandleGround()
	{
		_physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
		_physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash;
		// _numberAvailableDash = _stats.MaxNumberDash; // При касании земли возвращение рывков
	
		// _isJumping = false; // Сброс флага прыжка
		_coyoteUsable = true; // Установка флага разрешающего делать кайот прыжок
	
		_moveVelocity.y = _playerControllerStats.GroundGravity; // Гравитация на земле
	}
}

public class MovementModule 
{
	private readonly Rigidbody2D _rigidbody;

	private Vector2 _targetVelocity;
	private Vector2 _moveVelocity;
	private PhysicsContext _physicsContext;

	public MovementModule(Rigidbody2D rigidbody, PhysicsContext physicsContext) 
	{
		_rigidbody = rigidbody;
		_physicsContext = physicsContext;
	}

	public void HandleMovement(Vector2 _moveDirection, float speed, float acceleration, float deceleration)
	{
		_moveVelocity = _physicsContext.MoveVelocity;
			
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
		
		_physicsContext.MoveVelocity = _moveVelocity;

		// Debug.Log(_physicsContext.MoveVelocity);
	}
}
public class DashModule 
{
	private readonly Rigidbody2D _rigidbody;

	private Vector2 _moveVelocity;
	private PhysicsContext _physicsContext;
	private readonly PlayerControllerStats _playerControllerStats;
	private CountdownTimer _dashTimer;
	
	private Vector2 _dashDirection;
	private readonly TurnChecker _turnChecker;
	
	private bool IsFacingRight => _turnChecker.IsFacingRight;

	public DashModule(Rigidbody2D rigidbody, PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, TurnChecker turnChecker, CountdownTimer dashTimer) 
	{
		_rigidbody = rigidbody;
		_physicsContext = physicsContext;
		_playerControllerStats = playerControllerStats;
		_dashTimer = dashTimer;
		_turnChecker = turnChecker;
	}
	
	public void HandleDash(Vector2 _moveDirection)
	{
		_moveVelocity = _physicsContext.MoveVelocity;

		// Изменение скорости по оси X для совершение рывка
		_moveVelocity.x = _dashDirection.x * _playerControllerStats.DashVelocity;

		// Если скорость по y не равна 0, применяем рывок в оси Y
		if (_dashDirection.y != 0)
		{
			_moveVelocity.y = _dashDirection.y * _playerControllerStats.DashVelocity;
		}

		// Отмена рывка, если игрок проживаем противоположное направление
		if (_dashDirection == -_moveDirection)
		{
			OnExitDash();
		}

		// Применение гравитации во время рывка
		_physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.DashGravityMultiplayer);
		
		_physicsContext.MoveVelocity = _moveVelocity;
	}

	// Метод вызываемый при входе в состояние рывка
	public void OnEnterDash()
	{
		_dashTimer.Start(); // Запуск таймера рывка
		// _dashKeyIsPressed = false; // Сброс флага нажатия клавиши
		_physicsContext.NumberAvailableDash -= 1;
		// _numberAvailableDash -= 1; // Уменьшение количество оставшихся рывков
		_moveVelocity.y = 0f; // Сброс скорости по Y, для расчета правильного направления рывка
		_physicsContext.MoveVelocity = Vector2.zero;
	}

	// Метод вызываемый при выходе из состояния рывка
	public void OnExitDash()
	{
		_dashTimer.Stop(); // Остановка таймера
		_dashTimer.Reset(); // Сброс таймера
	}

	// Расчет направления рывка
	public void CalculateDashDirection(InputReader input)
	{
		_dashDirection = input.Direction;
		// _dashDirection = _moveDirection;
		
		_dashDirection = GetClosestDirection(_dashDirection); // Поиск ближайшего допустимого направления
	}
	
	// Метод для поиска ближайшего направления рывка
	private Vector2 GetClosestDirection(Vector2 targetDirection)
	{
		Vector2 closestDirection = Vector2.zero; // Начальное значение для ближайшего направления
		float minDistance = float.MaxValue;      // Минимальная дистанция для поиска ближайшего направления

		// Перебор всех допустимых направления в общем массиве направлений
		foreach (var dashDirection in _playerControllerStats.DashDirections)
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
		// return closestDirection == Vector2.zero ? (IsFacingRight ? Vector2.right : Vector2.left) : closestDirection;
		return closestDirection == Vector2.zero ? (IsFacingRight ? Vector2.right : Vector2.left) : closestDirection;

	}
	
	// Проверка является ли направление диагональным
	private bool IsDiagonal(Vector2 direction)
	{
		return Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1;
	}
}

public class PlayerPhysicsController
{
	private readonly Rigidbody2D _rigidbody;
	private readonly CollisionsChecker _collisionsChecker;
	private readonly PlayerControllerStats _stats;
	private readonly TurnChecker _turnChecker;

	private readonly CountdownTimer _jumpCoyoteTimer;
	private readonly CountdownTimer _jumpBufferTimer;
	private readonly CountdownTimer _dashTimer;

	private readonly MovementModule _movementModule;
	private readonly GroundModule _groundModule;
	
	public readonly JumpModule _jumpModule; // FIXME
	public readonly FallModule _fallModule; // FIXME
	public readonly DashModule _dashModule; // FIXME

	private readonly PlayerController _playerController;
	public readonly PhysicsContext _physicsContext;

	public Vector2 _moveVelocity;
	private Vector2 _targetVelocity;

	public PlayerPhysicsController(
		Rigidbody2D rigidbody,
		CountdownTimer jumpCoyoteTimer,
		CountdownTimer jumpBufferTimer,
		CollisionsChecker collisionsChecker,
		PlayerControllerStats stats,
		PlayerController playerController,
		CountdownTimer dashTimer,
		TurnChecker turnChecker)
	{
		_rigidbody = rigidbody;
		_jumpCoyoteTimer = jumpCoyoteTimer;
		_jumpBufferTimer = jumpBufferTimer;
		_collisionsChecker = collisionsChecker;
		_stats = stats;
		_playerController = playerController;
		_dashTimer = dashTimer;
		_turnChecker = turnChecker;

		_physicsContext = new PhysicsContext();

		_movementModule = new MovementModule(_rigidbody, _physicsContext);
		_groundModule = new GroundModule(_rigidbody, _stats, _physicsContext);
		_jumpModule = new JumpModule(_physicsContext, _rigidbody, _collisionsChecker, _stats, _jumpCoyoteTimer, _jumpBufferTimer);
		_fallModule = new FallModule(_physicsContext, _rigidbody, _collisionsChecker, _stats, _jumpCoyoteTimer, _jumpBufferTimer);
		_dashModule = new DashModule(_rigidbody, _physicsContext, _stats, _turnChecker, _dashTimer);
	}


	public void ApplyMovement()
	{
		_rigidbody.linearVelocity = _physicsContext.MoveVelocity;
	}

	public void HandleMovement(Vector2 moveDirection, float speed, float acceleration, float deceleration)
	{
		_movementModule.HandleMovement(moveDirection, speed, acceleration, deceleration);
	}

	public void HandleGround()
	{
		_groundModule.HandleGround();
	}

	public void HandleJump() => _jumpModule.HandleJump(_playerController.input.JumpState);

	public void HandleFalling() => _fallModule.HandleFalling(_playerController.input.JumpState);

	public void HandleDash(Vector2 moveDirection)
	{
		_dashModule.HandleDash(moveDirection);
	}

	public void IsReadyToFall() => _jumpModule.CanFall();
}
