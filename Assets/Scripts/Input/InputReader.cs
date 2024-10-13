using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

[CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
	public event UnityAction<Vector2> Move = delegate { };
	public event UnityAction<bool> Jump = delegate { };

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

	public void OnMove(InputAction.CallbackContext context)
	{
		Move.Invoke(context.ReadValue<Vector2>());
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		// if (context.phase == InputActionPhase.Performed)
		// {
		// 	Debug.Log($"{context.phase} + Jump");
		// 	Jump.Invoke();
		// }
		switch (context.phase)
		{
			case InputActionPhase.Started:
				Jump.Invoke(true);
				break;
			case InputActionPhase.Canceled:
				Jump.Invoke(false);
				break;
		}

		// Debug.Log($"{context.phase} + Jump");
	}
}
