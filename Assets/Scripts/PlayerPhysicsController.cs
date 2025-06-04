using System;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

public class PhysicsContext 
{
	public Vector2 MoveVelocity { get; set; }
	public bool VariableJumpHeight { get; set; }
	public bool WasWallSliding {get; set;}

	public float NumberAvailableJumps { get; set; }
	public float NumberAvailableDash { get; set; }
	
	public Vector2 ApplyGravity(Vector2 moveVelocity, float gravity, float gravityMultiplayer)
	{
		// Применение гравитации
		moveVelocity.y -= gravity * gravityMultiplayer * Time.fixedDeltaTime;
		return moveVelocity;
	}
	
	// public void CoyoteTimerStart()
	// {
	// 	_jumpCoyoteTimer.Start();
	// }
	
	// public void HandleGround()
	// {
	// 	NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
	// 	NumberAvailableDash = _playerControllerStats.MaxNumberDash; // При касании земли возвращение рывков
	//
	// 	MoveVelocity = _playerControllerStats.GroundGravity; // Гравитация на земле 
	// }
}
public class GroundModule
{
	private readonly Rigidbody2D _rigidbody;

	private readonly PlayerControllerStats _playerControllerStats;
	private readonly PhysicsContext _physicsContext;
	
	private Vector2 _moveVelocity;

	public GroundModule(PlayerControllerStats playerControllerStats , PhysicsContext physicsContext) 
	{
		_playerControllerStats = playerControllerStats;
		_physicsContext = physicsContext;
	}

	public void HandleGround()
	{
		_physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
		_physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash; // При касании земли возвращение рывков

		_moveVelocity.y = _playerControllerStats.GroundGravity; // Гравитация на земле 

		_physicsContext.MoveVelocity = _moveVelocity;
	}
}
public class JumpModule
{
	public JumpModule(PhysicsContext physicsContext, CollisionsChecker collisionsChecker, PlayerControllerStats playerControllerStats, CountdownTimer jumpCoyoteTimer, CountdownTimer jumpBufferTimer)
	{
		_collisionsChecker = collisionsChecker;
		_playerControllerStats = playerControllerStats;

		_jumpCoyoteTimer = jumpCoyoteTimer;
		_jumpBufferTimer = jumpBufferTimer;

		_physicsContext = physicsContext;
	}

	private readonly PhysicsContext _physicsContext;
	private readonly CollisionsChecker _collisionsChecker;
	private readonly PlayerControllerStats _playerControllerStats;
	private readonly CountdownTimer _jumpCoyoteTimer;
	private readonly CountdownTimer _jumpBufferTimer;
	
	private Vector2 _moveVelocity;
	private bool _variableJumpHeight;
	private bool _positiveMoveVelocity;
	private float _numberAvailableJumps;
	
	private bool _jumpKeyReleased;

	public void Test1Update(InputButtonState jumpState)
	{
		// Обработка ввода
		if (jumpState.WasPressedThisFrame)
		{
			_jumpBufferTimer.Start();
		}
		if (jumpState.WasReleasedThisFrame)
		{
			_jumpKeyReleased = true;
		}
	}

	public void Test2FixedUpdate(InputButtonState jumpState)
	{
		_moveVelocity = _physicsContext.MoveVelocity;

		if (_jumpBufferTimer.IsFinished) { _physicsContext.VariableJumpHeight = false; }
		if (_moveVelocity.y > 0f) { _positiveMoveVelocity = true; }

		if (_jumpBufferTimer.IsRunning && (_collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning || _physicsContext.NumberAvailableJumps > 0f))
		{
			if (!_collisionsChecker.IsGrounded && _jumpCoyoteTimer.IsFinished)
			{
				_physicsContext.NumberAvailableJumps -= 1f;
			}
			ExecuteJump();
			if (_jumpBufferTimer.IsRunning && !jumpState.IsHeld)
				ExecuteVariableJumpHeight();
			_physicsContext.NumberAvailableJumps -= 1;
			_jumpBufferTimer.Stop();
			_jumpCoyoteTimer.Stop();
			_jumpCoyoteTimer.Reset();
			_jumpBufferTimer.Reset();
		}

		if (_jumpKeyReleased)
		{
			ExecuteVariableJumpHeight();
			_jumpKeyReleased = false;
		}

		_moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.JumpGravityMultiplayer);
		_physicsContext.MoveVelocity = _moveVelocity;
	}
	
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
	
	private readonly Rigidbody2D _rigidbody;
	private readonly CollisionsChecker _collisionsChecker;
	private readonly PlayerControllerStats _playerControllerStats;
	
	private readonly CountdownTimer _jumpCoyoteTimer;
	private readonly CountdownTimer _jumpBufferTimer;

	private readonly PhysicsContext _physicsContext;

	private Vector2 _moveVelocity;
	private bool _coyoteUsable;

	public void Test(InputButtonState jumpState)
	{
		if (jumpState.WasPressedThisFrame) { _jumpBufferTimer.Start(); }
	}
	
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
			_moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.jumpHangGravityMult);

		} // Гравитация в прыжке (Гравитация если удерживается кнопка прыжка)
		else if (jumpState.IsHeld)
		{
			_moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.JumpGravityMultiplayer);

		} // Гравитация в падении
		else
		{
			_moveVelocity = _physicsContext.ApplyGravity(_moveVelocity, _playerControllerStats.Gravity, _playerControllerStats.FallGravityMultiplayer);
		}

		// Ограничение максимальной скорости падения
		_moveVelocity.y = Mathf.Clamp(_moveVelocity.y, -_playerControllerStats.maxFallSpeed, 50f);
		
		_physicsContext.MoveVelocity = _moveVelocity;
		
		// ApplyMovement();
	}


	// Метод отвечающий за запуск таймера кайота при входе в состояние падения
	public void CoyoteTimerStart()
	{
		// При спуске с платформы запускаем таймер кайота прыжка
		// if (_coyoteUsable && !_isJumping) // TODO только на запуске 
		// if (_coyoteUsable) // TODO только на запуске 
		// {
		// 	_coyoteUsable = false;
		// 	_jumpCoyoteTimer.Start();
		// }
		
		_jumpCoyoteTimer.Start();

	}

	// Метод вызываемый при выходе из состояния падения
	public void OnExitFall() // FIXME 
	{
		// Когда персонаж оказывается на земле, вернуть все флаги, которые обновляются на земле
		// Также используется для конкретной реализации буферного прыжка в StateMachine
		// if (_collisionsChecker.IsGrounded) HandleGround();
		if (_collisionsChecker.IsGrounded) HandleGround();

		// if (_collisionsChecker.IsTouchingWall) _isRunning = false; // FIXME
	}
	
	private void HandleGround()
	{
		_physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps; // При касании земли возвращение прыжков
		_physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash;
	
		_coyoteUsable = true; // Установка флага разрешающего делать кайот прыжок
	
		_moveVelocity.y = _playerControllerStats.GroundGravity; // Гравитация на земле
	}
}
public class MovementModule 
{
	private Vector2 _targetVelocity;
	private Vector2 _moveVelocity;
	private readonly PhysicsContext _physicsContext;

	public MovementModule(PhysicsContext physicsContext) 
	{
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
	}
}
public class DashModule 
{
	private readonly PhysicsContext _physicsContext;
	private readonly PlayerControllerStats _playerControllerStats;
	private readonly CountdownTimer _dashTimer;
	private readonly TurnChecker _turnChecker;
	
	private Vector2 _moveVelocity;
	private Vector2 _dashDirection;
	
	private bool IsFacingRight => _turnChecker.IsFacingRight;

	public DashModule(PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, TurnChecker turnChecker, CountdownTimer dashTimer) 
	{
		_physicsContext = physicsContext;
		_playerControllerStats = playerControllerStats;
		_dashTimer = dashTimer;
		_turnChecker = turnChecker;
	}
	
	public void HandleDash(Vector2 _moveDirection)
	{
		_moveVelocity = _physicsContext.MoveVelocity;

		// Изменение скорости по оси X для совершения рывка
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
		_physicsContext.NumberAvailableDash -= 1;
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
	public void CalculateDashDirection(Vector2 _moveDirection) // Убрать из FallState и вызывать сразу в EnterState
	{
		// _dashDirection = input.Direction;
		_dashDirection = _moveDirection;
		
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
public class WallSlideModule
{
	private readonly PhysicsContext _physicsContext;
	private readonly PlayerControllerStats _playerControllerStats;
	private readonly TurnChecker _turnChecker;
	private readonly CountdownTimer _wallJumpTimer;
	
	private Vector2 _moveVelocity;
	private bool _wasWallSliding;
	
	private bool IsFacingRight => _turnChecker.IsFacingRight;

	public WallSlideModule(PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, TurnChecker turnChecker, CountdownTimer wallJumpTimer) 
	{
		_physicsContext = physicsContext;
		_playerControllerStats = playerControllerStats;
		_turnChecker = turnChecker;
		_wallJumpTimer = wallJumpTimer;
	}
	
	public void HandleWallInteraction(Vector2 _moveDirection, InputButtonState jumpInputButtonState)
	{
		_moveVelocity = _physicsContext.MoveVelocity;

		HandleWallSlide(_moveDirection);
		
		if (jumpInputButtonState.WasPressedThisFrame)
		{
			HandleWallJump(_moveDirection);
		}
		
		_physicsContext.MoveVelocity = _moveVelocity;

		// if (_jumpKeyWasPressed)
		// {
		// 	HandleWallJump(Vector2 _moveDirection);
		// }
	}

	// Обработка скольжения по стене WallSlide
	public void HandleWallSlide(Vector2 _moveDirection)
	{
		_moveVelocity = _physicsContext.MoveVelocity;
		// Плавное Изменение Y на стене,для скольжения 
		// Lerp зависит - 1. Начальная скорость скольжения (задается в Enter), 2 - макс. скорость скольжения, 3 - Deceleration скольжения
		_moveVelocity.y = Mathf.Lerp(_moveVelocity.y, -_playerControllerStats.WallSlideSpeedMax, _playerControllerStats.WallSlideDeceleration * Time.fixedDeltaTime); //FIXME

		HandleWallJumpTimer(_moveDirection);

		// Если ввод обратный вводу стены, ввод не равен 0, таймер не идет и на стене, запускаем таймер, отвечающий остаток времени на стене
		// Запуск таймера при попытке слезть со стены, помогает выполнить прыжок от стены
		if (_moveDirection.x != CalculateWallDirectionX() && _moveDirection.x != 0f && !_wallJumpTimer.IsRunning && _physicsContext.WasWallSliding)
		{
			_wallJumpTimer.Start();
		}

		_physicsContext.MoveVelocity = _moveVelocity;
	}

	// Метод управления таймером
	private void HandleWallJumpTimer(Vector2 _moveDirection)
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
				_physicsContext.WasWallSliding = false;
			}
		}
	}

	// Метод вычисляет направление стены по X
	public float CalculateWallDirectionX()
	{
		return IsFacingRight ? 1f : -1f;
	}

	// Применение прыжка со стены WallJump
	public void HandleWallJump(Vector2 _moveDirection)
	{
		_moveVelocity = _physicsContext.MoveVelocity;

		// Расчет направление персонажа по X
		float wallDirectionX = CalculateWallDirectionX();
	
		// Если ввод в сторону стены
		if (_moveDirection.x == wallDirectionX)
		{
			Debug.Log("В СТОРОНУ СТЕНЫ");

			// Прыжок вверх по стене
			_moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpClimb.x, _playerControllerStats.WallJumpClimb.y);
		}
		else if (_moveDirection.x == 0f) // Если ввод равен 0
		{
			Debug.Log("ПО СТЕНЕ");

			// Прыжок от стены
			_moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallJumpOff.x, _playerControllerStats.WallJumpOff.y);
		}
		else // Если ввод сторону от стены, обратную сторону
		{
			Debug.Log("ОТ СТЕНЫ");

			// Прыжок в сторону от стены
			_moveVelocity = new Vector2(-wallDirectionX * _playerControllerStats.WallLeap.x, _playerControllerStats.WallLeap.y);
		}
		
		_physicsContext.MoveVelocity = _moveVelocity;
	}

	// Метод вызываемый при входе в состояние wallJump/Slide
	
	public void OnEnterWallSliding()
	{
		// Начальная скорость скольжения, x = 0 - персонаж падает вниз после окончания скольжения, то есть не сохраняет скорость
		_moveVelocity = new Vector2(0f, -_playerControllerStats.StartVelocityWallSlide);
		_physicsContext.MoveVelocity = _moveVelocity;
		
		// Сброс таймера
		_wallJumpTimer.Reset();

		// Флаг скольжения по стене
		_physicsContext.WasWallSliding = true;

		// Обнуления максКолвоПрыжков, максКолвоРывков
		_physicsContext.NumberAvailableJumps = _playerControllerStats.MaxNumberJumps;
		_physicsContext.NumberAvailableDash = _playerControllerStats.MaxNumberDash;
	}

	// Метод вызываемый при выходе в состояние wallJump/Slide
	public void OnExitWallSliding()
	{
		// Остановка таймера
		_wallJumpTimer.Stop();
		
		// Сброс флага скольжения
		_physicsContext.WasWallSliding = false;
	}
	
	
}
public class CrouchModule
{
	private readonly PhysicsContext _physicsContext;
	private readonly PlayerControllerStats _playerControllerStats;
	private readonly CapsuleCollider2D _capsuleCollider;
	private readonly Transform _spriteTransform;

	private Vector2 _moveVelocity;
	private Vector2 _dashDirection;

	private Vector2 normalHeight => _capsuleCollider.size;

	public CrouchModule(PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, CapsuleCollider2D capsuleCollider, Transform spriteTransform) 
	{
		_physicsContext = physicsContext;
		_playerControllerStats = playerControllerStats;
		_capsuleCollider = capsuleCollider;
		_spriteTransform = spriteTransform;
	}
	
	public void OnEnterCrouch()
	{
		SetCrouchState(true);
	}
	
	// Метод вызываемый при выходе из состояния приседа
	public void OnExitCrouch(InputButtonState crouchRollButtonState)
	{
		// if (_dashKeyIsPressed) return;
		
		if (crouchRollButtonState.WasPressedThisFrame) return;
		
		SetCrouchState(false);
	}

	// Метод который регулирует высоту спрайта и коллайдера в зависиомсти сидит ли персонаж или стоит
	private void SetCrouchState(bool isCrouching)
	{
		// _isSitting = isCrouching;
		// Еси персонаж сидит его высота равна высоте приседа, если нет обычной высоте
		var height = isCrouching ? _playerControllerStats.CrouchHeight : normalHeight.x;
		// Еси персонаж сидит его оффсет равен оффсету приседа, если нет то нулю
		var offset = isCrouching ? -_playerControllerStats.CrouchOffset : 0;

		// Настройка коллайдера
		_capsuleCollider.size = new Vector2(_capsuleCollider.size.x, height);
		_capsuleCollider.offset = new Vector2(_capsuleCollider.offset.x, offset);

		// Настройка спрайта
		_spriteTransform.localScale = isCrouching ? new Vector2(1f, _playerControllerStats.CrouchHeight) : Vector2.one;
		_spriteTransform.localPosition = isCrouching ? new Vector2(_spriteTransform.localPosition.x, offset) : Vector2.zero;
	}
}
public class CrouchRollModule
{
	private readonly PhysicsContext _physicsContext;
	private readonly PlayerControllerStats _playerControllerStats;
	private readonly CapsuleCollider2D _capsuleCollider;
	private readonly Transform _spriteTransform;
	private readonly CountdownTimer _crouchRollTimer;
	private readonly TurnChecker _turnChecker;
	
	private bool IsFacingRight => _turnChecker.IsFacingRight;



	private Vector2 _moveVelocity;
	private Vector2 _crouchRollDirection;


	public CrouchRollModule(PhysicsContext physicsContext, PlayerControllerStats playerControllerStats, CapsuleCollider2D capsuleCollider, Transform spriteTransform, CountdownTimer crouchRollTimer, TurnChecker turnChecker) 
	{
		_physicsContext = physicsContext;
		_playerControllerStats = playerControllerStats;
		_capsuleCollider = capsuleCollider;
		_spriteTransform = spriteTransform;
		_crouchRollTimer = crouchRollTimer;
		_turnChecker = turnChecker;
	}
	
	public void CrouchRoll()
	{
		_moveVelocity = _physicsContext.MoveVelocity;
		_moveVelocity.x = _crouchRollDirection.x * _playerControllerStats.CrouchRollVelocity;
		_physicsContext.MoveVelocity = _moveVelocity;
	}
	
	// Метод вызываемый при входе в состояние кувырка в приседе
	public void OnEnterCrouchRoll()
	{
		// SetCrouchState(true);

		_crouchRollTimer.Start();
		// Сохранение направления кувырка
		_crouchRollDirection = IsFacingRight ? Vector2.right : Vector2.left;
		// _dashKeyIsPressed = false;
	}
	
	// Метод вызываемый при выходе из кувырка в приседе
	public void OnExitCrouchRoll()
	{
		// SetCrouchState(false);

		_crouchRollTimer.Stop();
		_crouchRollTimer.Reset();
		// Если кувырок сделан с уступа вернуть высоту коллайдера и спрайта
		// if (!_collisionsChecker.IsGrounded) // FIXME
		// 	SetCrouchState(false); 
	}
}

public class PlayerPhysicsController
{
	public readonly MovementModule MovementModule;
	public readonly GroundModule GroundModule;
	public readonly JumpModule JumpModule;
	public readonly FallModule FallModule; 
	public readonly DashModule DashModule;
	public readonly WallSlideModule WallSlideModule;
	public readonly CrouchModule CrouchModule;
	public readonly CrouchRollModule CrouchRollModule;
	
	public readonly PhysicsContext PhysicsContext;

	private readonly Rigidbody2D _rigidbody;
	private readonly CollisionsChecker _collisionsChecker;
	private readonly PlayerControllerStats _stats;
	private readonly TurnChecker _turnChecker;

	private readonly CountdownTimer _jumpCoyoteTimer;
	private readonly CountdownTimer _jumpBufferTimer;
	private readonly CountdownTimer _dashTimer;
	private readonly CountdownTimer _wallJumpTimer;

	private readonly PlayerController _playerController;
	
	public PlayerPhysicsController(
		Rigidbody2D rigidbody,
		CountdownTimer jumpCoyoteTimer,
		CountdownTimer jumpBufferTimer,
		CollisionsChecker collisionsChecker,
		PlayerControllerStats stats,
		PlayerController playerController,
		CountdownTimer dashTimer,
		TurnChecker turnChecker,
		CountdownTimer wallJumpTimer,
		CountdownTimer crouchRollTimer)
	{
		_rigidbody = rigidbody;
		_jumpCoyoteTimer = jumpCoyoteTimer;
		_jumpBufferTimer = jumpBufferTimer;
		_collisionsChecker = collisionsChecker;
		_stats = stats;
		_playerController = playerController;
		_dashTimer = dashTimer;
		_turnChecker = turnChecker;
		_wallJumpTimer = wallJumpTimer;

		PhysicsContext = new PhysicsContext();

		MovementModule = new MovementModule(PhysicsContext);
		GroundModule = new GroundModule(_stats, PhysicsContext);
		JumpModule = new JumpModule(PhysicsContext, _collisionsChecker, _stats, _jumpCoyoteTimer, _jumpBufferTimer);
		FallModule = new FallModule(PhysicsContext, _rigidbody, _collisionsChecker, _stats, _jumpCoyoteTimer, _jumpBufferTimer);
		DashModule = new DashModule(PhysicsContext, _stats, _turnChecker, _dashTimer);
		WallSlideModule = new WallSlideModule(PhysicsContext, _stats, _turnChecker, _wallJumpTimer);
		CrouchModule = new CrouchModule(PhysicsContext, _stats, playerController._capsuleCollider, playerController.spriteTransform);
		CrouchRollModule = new CrouchRollModule(PhysicsContext, _stats, playerController._capsuleCollider, playerController.spriteTransform, crouchRollTimer, turnChecker);

	}

	public void ApplyMovement()
	{
		_rigidbody.linearVelocity = PhysicsContext.MoveVelocity;
	}
	
	public void BumpedHead()
	{
		// Проверка не ударился ли персонаж головой платформы
		if (_collisionsChecker.BumpedHead)
		{
			var moveVelocity = PhysicsContext.MoveVelocity;
			// Отправить персонажа вниз
			moveVelocity.y = Mathf.Min(0, moveVelocity.y);
			PhysicsContext.MoveVelocity = moveVelocity;
		}
	}

	#region Pattern facade

	// public void HandleMovement(Vector2 moveDirection, float speed, float acceleration, float deceleration)
	// {
	// 	_movementModule.HandleMovement(moveDirection, speed, acceleration, deceleration);
	// }
	//
	// public void HandleGround()
	// {
	// 	_groundModule.HandleGround();
	// }
	//
	// public void HandleJump() => _jumpModule.HandleJump(_playerController.input.JumpState);
	//
	// public void HandleFalling() => _fallModule.HandleFalling(_playerController.input.JumpState);
	//
	// public void HandleDash(Vector2 moveDirection)
	// {
	// 	_dashModule.HandleDash(moveDirection);
	// }
	//
	// public void IsReadyToFall() => _jumpModule.CanFall();

	#endregion
}
