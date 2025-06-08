using UnityEngine;

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
        // if (_jumpBufferTimer.IsRunning || _collisionsChecker.IsGrounded || _jumpCoyoteTimer.IsRunning || _physicsContext.NumberAvailableJumps > 0f)
        {
            if (!_collisionsChecker.IsGrounded && !_jumpCoyoteTimer.IsFinished)
            {
                _physicsContext.NumberAvailableJumps -= 1f;
            }
            
            ExecuteJump();
            
            // if (_jumpBufferTimer.IsRunning && _jumpKeyReleased)
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
        Debug.Log("1");
        // Изменения Y на высоту прыжка
        _moveVelocity.y = _playerControllerStats.MaxJumpVelocity;
    }
	
    // Метод для выполнения неполного прыжка
    private void ExecuteVariableJumpHeight()
    {
        Debug.Log("2");

        // Изменения Y на минимальную высоту прыжка
        if (_moveVelocity.y > _playerControllerStats.MinJumpVelocity)
        {
            _moveVelocity.y = _playerControllerStats.MinJumpVelocity;
        }
    }
}