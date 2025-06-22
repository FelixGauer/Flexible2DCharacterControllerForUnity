using UnityEngine.InputSystem;

public class InputButtonState
{
    public bool IsHeld { get; private set; }
    public bool WasPressedThisFrame { get; private set; }
    public bool WasReleasedThisFrame { get; private set; }
    
    // // Инвертированные свойства
    // public bool IsHeldInverted => !IsHeld;
    // public bool WasPressedThisFrameInverted => WasReleasedThisFrame;
    // public bool WasReleasedThisFrameInverted => WasPressedThisFrame;

    public void Update(InputAction.CallbackContext context)
    {
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