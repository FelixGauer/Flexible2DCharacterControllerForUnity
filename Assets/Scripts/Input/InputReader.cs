using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
	private PlayerInputActions _inputActions;

    private readonly VectorInputHandler _moveHandler = new VectorInputHandler();
    private readonly ButtonInputHandler _dashHandler = new ButtonInputHandler();
    private readonly ButtonInputHandler _jumpHandler = new ButtonInputHandler();
    private readonly ButtonInputHandler _runHandler = new ButtonInputHandler();
    private readonly ButtonInputHandler _crouchHandler = new ButtonInputHandler();

    // События для уведомления о изменениях ввода
    public event Action<Vector2> OnMoveChanged;
    public event Action<InputButtonState> OnDashChanged;
    public event Action<InputButtonState> OnJumpChanged;
    public event Action<InputButtonState> OnRunChanged;
    public event Action<InputButtonState> OnCrouchChanged;

    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Player.SetCallbacks(this);
        }
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveHandler.Update(context);
        OnMoveChanged?.Invoke(_moveHandler.Value);
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        _dashHandler.Update(context);
        OnDashChanged?.Invoke(_dashHandler.State);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpHandler.Update(context);
        OnJumpChanged?.Invoke(_jumpHandler.State);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        _runHandler.Update(context);
        OnRunChanged?.Invoke(_runHandler.State);
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        _crouchHandler.Update(context);
        OnCrouchChanged?.Invoke(_crouchHandler.State);
    }

    public void OnCrouchRoll(InputAction.CallbackContext context) => OnDash(context);

    // Методы доступа с проверками на null
    public Vector2 GetMoveDirection() => _moveHandler != null ? _moveHandler.Value : Vector2.zero;
    public InputButtonState GetDashState() => _dashHandler != null ? _dashHandler.State : new InputButtonState();
    public InputButtonState GetJumpState() => _jumpHandler != null ? _jumpHandler.State : new InputButtonState();
    public InputButtonState GetRunState() => _runHandler != null ? _runHandler.State : new InputButtonState();
    public InputButtonState GetCrouchState() => _crouchHandler != null ? _crouchHandler.State : new InputButtonState();


    // public PlayerControllerStats PlayerControllerStats;
    //
    // /// <summary>
    // /// Определяет, должен ли игрок бежать в данный момент, основываясь на настройках движения
    // /// </summary>
    // public bool ShouldRun()
    // {
    //     if (PlayerControllerStats.movementSettings == null) return GetRunState().IsHeld;
    //     
    //     switch (PlayerControllerStats.movementSettings.defaultMovementMode)
    //     {
    //         case MovementMode.Walk:
    //             // Ходьба по умолчанию - бежать при зажатой клавише
    //             return GetRunState().IsHeld;
    //             
    //         case MovementMode.Run:
    //             // Бег по умолчанию - идти при зажатой клавише (инвертированная логика)
    //             return !GetRunState().IsHeld;
    //             
    //         default:
    //             return GetRunState().IsHeld;
    //     }
    // }
    //
    // /// <summary>
    // /// Определяет, должен ли игрок идти обычным шагом
    // /// </summary>
    // public bool ShouldWalk()
    // {
    //     return !ShouldRun();
    // }

    // Сброс состояний кадра, вызывается внешним MonoBehaviour
    public void ResetFrameStates()
    {
        _dashHandler.State.ResetFrameState();
        _jumpHandler.State.ResetFrameState();
        _runHandler.State.ResetFrameState();
        _crouchHandler.State.ResetFrameState();
    }
}

public interface IInputHandler
{
	void Update(InputAction.CallbackContext context);
}

public class VectorInputHandler : IInputHandler
{
	public Vector2 Value { get; private set; }

	public void Update(InputAction.CallbackContext context)
	{
		Value = context.ReadValue<Vector2>();
	}
}

public class ButtonInputHandler : IInputHandler
{
	public InputButtonState State { get; private set; } = new InputButtonState();

	public void Update(InputAction.CallbackContext context)
	{
		State.Update(context);
	}
}


