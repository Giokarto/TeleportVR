using UnityEngine;

namespace InputDevices.Keyboard
{
    /// <summary>
    /// Fallback input system if no others are available.
    /// </summary>
    public class KeyboardInputSystem : InputSystem
    {
        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
                InvokeLeftMenuButton();
            if (Input.GetKeyDown(KeyCode.A))
                InvokeLeftPrimaryButton();
            if (Input.GetKeyDown(KeyCode.S))
                InvokeLeftSecondaryButton();
            if (Input.GetKeyDown(KeyCode.L))
                InvokeRightPrimaryButton();
            if (Input.GetKeyDown(KeyCode.K))
                InvokeRightSecondaryButton();
            
            if (Input.anyKeyDown)
                InvokeAnyButton();
        }
    }
}