using System;
using UnityEngine;

public class PlayerPhysicsController
{
	public PlayerPhysicsController(Rigidbody2D rigidbody, CountdownTimer jumpCoyoteTimer, CountdownTimer jumpBufferTimer, CollisionsChecker collisionsChecker, PlayerControllerStats stats)
	{
		_rigidbody = rigidbody;
		_jumpBufferTimer = jumpBufferTimer;
		_jumpCoyoteTimer = jumpCoyoteTimer;
		_collisionsChecker = collisionsChecker;
		_stats = stats;
	}
	
	private Rigidbody2D _rigidbody;
	private CollisionsChecker _collisionsChecker;
	private PlayerControllerStats _stats;

	public Vector2 _moveVelocity;
	private Vector2 _targetVelocity;
    
	// Регион отвечающий за Movement/Run
	#region Movement/Run
	
	public void ApplyMovement()
	{
		// Изменение координат игрока 
		_rigidbody.linearVelocity = _moveVelocity;
	}
	
	// Обработка движения игрока шаг/бег/присед/воздух	
	public void HandleMovement(Vector2 _moveDirection, float speed, float acceleration, float deceleration)
	{
		// Выход из состояния бега. Так как вход только при переходе в состояние бега, этот метод помогает реализовать логику
		// Того что в состояние бега можно войти только на земле, а выйти в любое время.
		// if (_isRunning) CheckExitRun(); // TODO

		// float speed = GetCurrentSpeed();
		// float acceleration = GetCurrentAcceleration();
		// float deceleration = GetCurrentDeceleration();

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

	
	// // Метод для получения текущей скорости
	// private float GetCurrentSpeed()
	// {
	// 	if (_isSitting) return stats.CrouchMoveSpeed;
	// 	if (_isRunning) return stats.RunSpeed;
	// 	return stats.MoveSpeed;
	// }
	//
	// // Метод для получения текущего ускорения
	// private float GetCurrentAcceleration()
	// {
	// 	if (!_collisionsChecker.IsGrounded) return stats.airAcceleration; // TODO
	// 	if (_isSitting) return stats.CrouchAcceleration;
	// 	if (_isRunning) return stats.RunAcceleration;
	// 	return stats.WalkAcceleration;
	// }
	//
	// // Метод для получения текущей замедления
	// private float GetCurrentDeceleration()
	// {
	// 	if (!_collisionsChecker.IsGrounded) return stats.airDeceleration; // TODO
	// 	if (_isSitting) return stats.CrouchDeceleration;
	// 	if (_isRunning) return stats.RunDeceleration;
	// 	return stats.WalkDeceleration;
	// }

	// #region Run
	//
	// // Метод вызываемый при входе в состояние бега
	// public void OnEnterRun()
	// {
	// 	// Нельзя войти в состояние бега находясь не на земле
	// 	if (_collisionsChecker.IsGrounded)
	// 	{
	// 		_isRunning = true;
	// 	}
	// }
	//
	// // Метод для выхода из бега. Данный метод помогает в реализации механики которая позволяет применять бег только на земле.
	// // То есть в воздухе не получится изменить переменную движения на переменную бега. А вот выйти из бега получится.
	// private void CheckExitRun()
	// {
	// 	if (!_runKeyIsPressed)
	// 	{
	// 		_isRunning = false;
	// 	}
	// }
	//
	// #endregion

	#endregion
	
	
	#region Jump
	
	public CountdownTimer _jumpCoyoteTimer;
	public CountdownTimer _jumpBufferTimer;
	
	private bool _variableJumpHeight;
	public bool _positiveMoveVelocity = false;
	
	public float _numberAvailableJumps;
	
	private bool _isJumping = false;
	public bool _isCutJumping = false;
	
	// Метод обработки прыжка
	public void HandleJump(bool _jumpKeyWasPressed, bool _jumpKeyWasLetGo)
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
		SetGravity(_stats.JumpGravityMultiplayer);
	}

	// Метод для выполнения прыжка
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

	// Метод для выполнения неполного прыжка
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
	
	
	//Calculate jump vars
	float AdjustedJumpHeight => _stats.maxJumpHeight * _stats.jumpHeightCompensationFactor;
	float gravity => 2f * AdjustedJumpHeight / MathF.Pow(_stats.timeTillJumpApex, 2f);
	float maxJumpVelocity => gravity * _stats.timeTillJumpApex;
	float minJumpVelocity => Mathf.Sqrt(2 * _stats.minJumpHeight * gravity);
	private void SetGravity(float gravityMulitplayer)
	{
		// Применение гравитации
		_moveVelocity.y -= gravity * gravityMulitplayer * Time.fixedDeltaTime;
	}
	
	// Регион отвечающий за Fall/Падение
	#region Fall

	// Метод отвечающий за обработку состояния падения
	public void HandleFalling(bool _jumpKeyWasPressed, bool _jumpKeyWasLetGo, bool _jumpKeyIsPressed)
	{
		// Проверка на удар головой об платформу
		// BumpedHead(); //FIXME

		// Запуск таймера прыжка в падении
		if (_jumpKeyWasPressed) { _jumpBufferTimer.Start(); }
		// Сохранение переменной для буферизации минимального прыжка
		if (_jumpBufferTimer.IsRunning && _jumpKeyWasLetGo) { _variableJumpHeight = true; }

		// Применнение гравитации
		// Гравитация в верхней точки прыжыка
		if (Mathf.Abs(_rigidbody.linearVelocity.y) < _stats.jumpHangTimeThreshold)
		{
			SetGravity(_stats.jumpHangGravityMult);
		} // Гравитация в прыжке (Гравитация если удерживается кнопка прыжка)
		else if (_jumpKeyIsPressed)
		{
			SetGravity(_stats.JumpGravityMultiplayer);
		} // Гравитация в падении
		else
		{
			SetGravity(_stats.FallGravityMultiplayer);
		}

		// Ограничение максимальной скорости падения
		_moveVelocity.y = Mathf.Clamp(_moveVelocity.y, -_stats.maxFallSpeed, 50f);
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

	#endregion
	
	private float _numberAvailableDash;
	private float maxYPosition;


	public void HandleGround()
	{
		// Debug.Log(maxYPosition);
		if (maxYPosition > 0)
		{
			maxYPosition = 0;
		}

		_numberAvailableJumps = _stats.MaxNumberJumps; // При касании земли возвращение прыжков
		_numberAvailableDash = _stats.MaxNumberDash; // При касании земли возвращение рывков

		_isJumping = false; // Сброс флага прыжка
		_coyoteUsable = true; // Установка флага разрешающего делать кайот прыжок

		_moveVelocity.y = _stats.GroundGravity; // Гравитация на земле
	}
	
	
	// #region Dash
	//
	// private Vector2 _dashDirection;
	//
	//
	// // Обработка состояния рывка
	// public void HandleDash()
	// {
	// 	// Изменение скорости по оси X для совершение рывка
	// 	_moveVelocity.x = _dashDirection.x * _stats.DashVelocity;
	//
	// 	// Если скорость по y не равна 0, применяем рывок в оси Y
	// 	if (_dashDirection.y != 0)
	// 	{
	// 		_moveVelocity.y = _dashDirection.y * _stats.DashVelocity;
	// 	}
	//
	// 	// Отмена рывка, если игрок проживаем противоположное направление
	// 	if (_dashDirection == -_moveDirection)
	// 	{
	// 		OnExitDash();
	// 	}
	//
	// 	// Применение гравитации во время рывка
	// 	SetGravity(stats.DashGravityMultiplayer);
	// }
	//
	// // Метод вызываемый при входе в состояние рывка
	// public void OnEnterDash()
	// {
	// 	_dashTimer.Start(); // Запуск таймера рывка
	// 	_dashKeyIsPressed = false; // Сброс флага нажатия клавиши
	// 	_numberAvailableDash -= 1; // Уменьшение количество оставшихся рывков
	// 	_moveVelocity.y = 0f; // Сброс скорости по Y, для расчета правильного направления рывка
	// }
	//
	// // Метод вызываемый при выходе из состояния рывка
	// public void OnExitDash()
	// {
	// 	_dashTimer.Stop(); // Остановка таймера
	// 	_dashTimer.Reset(); // Сброс таймера
	// }
	//
	// // Расчет направления рывка
	// public void CalculateDashDirection()
	// {
	// 	_dashDirection = input.Direction;
	// 	_dashDirection = GetClosestDirection(_dashDirection); // Поиск ближайшего допустимого направления
	// }
	//
	// // Метод для поиска ближайшего направления рывка
	// private Vector2 GetClosestDirection(Vector2 targetDirection)
	// {
	// 	Vector2 closestDirection = Vector2.zero; // Начальное значение для ближайшего направления
	// 	float minDistance = float.MaxValue;      // Минимальная дистанция для поиска ближайшего направления
	//
	// 	// Перебор всех допустимых направления в общем массиве направлений
	// 	foreach (var dashDirection in stats.DashDirections)
	// 	{
	// 		float distance = Vector2.Distance(targetDirection, dashDirection);
	//
	// 		// Проверка на диагональное направление
	// 		if (IsDiagonal(dashDirection))
	// 		{
	// 			distance = 1f;
	// 		}
	// 		// Если найдено близкое направление, обновляем ближайшее и минимальную дистанцию
	// 		if (distance < minDistance)
	// 		{
	// 			minDistance = distance;
	// 			closestDirection = dashDirection;
	// 		}
	// 	}
	//
	// 	// Если стоит на месте, применяем рывок в сторону поворота игрока, иначе в найденое ближайшее направление
	// 	return closestDirection == Vector2.zero ? (IsFacingRight ? Vector2.right : Vector2.left) : closestDirection;
	// }
	//
	// // Проверка является ли направление диагональным
	// private bool IsDiagonal(Vector2 direction)
	// {
	// 	return Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1;
	// }
	//
	// #endregion

}