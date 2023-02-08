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
            if (Input.GetKeyDown(KeyCode.X))
                InvokeLeftPrimaryButton();
            if (Input.GetKeyDown(KeyCode.Y))
                InvokeLeftSecondaryButton();
            if (Input.GetKeyDown(KeyCode.A))
                InvokeRightPrimaryButton();
            if (Input.GetKeyDown(KeyCode.B))
                InvokeRightSecondaryButton();
            
            if (Input.anyKeyDown)
                InvokeAnyButton();
        }
    }
}