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
            if (Input.GetKey(KeyCode.M))
                InvokeLeftMenuButton();
            if (Input.GetKey(KeyCode.A))
                InvokeLeftPrimaryButton();
            if (Input.GetKey(KeyCode.S))
                InvokeLeftSecondaryButton();
            if (Input.GetKey(KeyCode.L))
                InvokeRightPrimaryButton();
            if (Input.GetKey(KeyCode.K))
                InvokeRightSecondaryButton();
            
            if (Input.anyKey)
                InvokeAnyButton();
        }
    }
}