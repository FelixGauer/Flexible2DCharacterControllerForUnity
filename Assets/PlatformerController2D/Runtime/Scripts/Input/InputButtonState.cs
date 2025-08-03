using System;
using UnityEngine.InputSystem;

public class InputButtonState
{
    public bool IsHeld { get; private set; }
    public bool WasPressedThisFrame { get; private set; }
    public bool WasReleasedThisFrame { get; private set; }

    private void Press()
    {
        if (!IsHeld)
        {
            IsHeld = true;
            WasPressedThisFrame = true;
        }
    }
    
    public void Update(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Press();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            Release();
        }
    }

    public void ResetFrameState()
    {
        WasPressedThisFrame = false;
        WasReleasedThisFrame = false;
    }

    private void Release()
    {
        if (IsHeld)
        {
            IsHeld = false;
            WasReleasedThisFrame = true;
        }
    }
}

// public class InputButtonState2
// {
//     public bool IsHeld { get; private set; }
//     public bool WasPressedThisFrame { get; private set; }
//     public bool WasReleasedThisFrame { get; private set; }
//
//     public void Update(InputAction.CallbackContext context)
//     {
//         switch (context.phase)
//         {
//             case InputActionPhase.Started:
//                 if (!IsHeld)
//                 {
//                     IsHeld = true;
//                     WasPressedThisFrame = true;
//                 }
//                 break;
//
//             case InputActionPhase.Canceled:
//                 if (IsHeld)
//                 {
//                     IsHeld = false;
//                     WasReleasedThisFrame = true;
//                 }
//                 break;
//         }
//     }
//
//     public void ResetFrameState()
//     {
//         WasPressedThisFrame = false;
//         WasReleasedThisFrame = false;
//     }
// }


