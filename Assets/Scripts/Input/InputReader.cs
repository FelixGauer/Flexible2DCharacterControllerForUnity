using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
	public event UnityAction<Vector2> Move = delegate { };
	public event UnityAction<bool> Jump = delegate { };
	public event UnityAction<bool> Dash = delegate { };
	public event UnityAction<bool> Crouch = delegate { };
	public event UnityAction<bool> Run = delegate { };
	public event UnityAction<bool> CrouchRoll = delegate { };

	private PlayerInputActions _inputActions;

	public Vector3 Direction => _inputActions.Player.Move.ReadValue<Vector2>();

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
		Move.Invoke(context.ReadValue<Vector2>());
	}

	// public void OnJump(InputAction.CallbackContext context)
	// {
	// 	switch (context.phase)
	// 	{
	// 		case InputActionPhase.Started:
	// 			Jump.Invoke(true);
	// 			break;
	// 		case InputActionPhase.Canceled:
	// 			Jump.Invoke(false);
	// 			break;
	// 	}
	//
	// }
	
	// public void OnDash(InputAction.CallbackContext context)
	// {
	// 	if (context.performed)
	// 	{
	// 		// Отправляем true только один раз, когда кнопка реально «нажата»
	// 		Dash.Invoke(true);
	// 	}
	// 	else if (context.canceled)
	// 	{
	// 		// Это когда кнопку отпустили
	// 		Dash.Invoke(false);
	// 	}
	// }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        switch (context.phase)
		{
			case InputActionPhase.Started:
				Crouch.Invoke(true);
				break;
			case InputActionPhase.Canceled: //FIXME
				Crouch.Invoke(false);
				break;
		}
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
		{
			case InputActionPhase.Started:
				Run.Invoke(true);
				break;
			case InputActionPhase.Canceled: //FIXME
				Run.Invoke(false);
				break;
		}
    }

    public void OnCrouchRoll(InputAction.CallbackContext context)
    {
        switch (context.phase)
		{
			case InputActionPhase.Started:
				CrouchRoll.Invoke(true);
				break;
			case InputActionPhase.Canceled:
				CrouchRoll.Invoke(false);
				break;
		}
    }
    
    public InputButtonState DashInputButtonState { get; private set; } = new();
    public InputButtonState JumpInputButtonState { get; private set; } = new();

    public void OnDash(InputAction.CallbackContext context) => DashInputButtonState.Update(context);

    public void OnJump(InputAction.CallbackContext context) => JumpInputButtonState.Update(context);
}


public class InputButtonState
{
	public bool IsHeld { get; private set; }
	public bool WasPressedThisFrame { get; private set; }
	public bool WasReleasedThisFrame { get; private set; }

	public void Update(InputAction.CallbackContext context)
	{
		// WasPressedThisFrame = false;
		// WasReleasedThisFrame = false;

		switch (context.phase)
		{
			case InputActionPhase.Started:
				if (!IsHeld)
				{
					IsHeld = true;
					WasPressedThisFrame = true;
				}
				break;

			case InputActionPhase.Canceled:
				if (IsHeld)
				{
					IsHeld = false;
					WasReleasedThisFrame = true;
				}
				break;
		}
	}

	public void ResetFrameState()
	{
		WasPressedThisFrame = false;
		WasReleasedThisFrame = false;
	}
}