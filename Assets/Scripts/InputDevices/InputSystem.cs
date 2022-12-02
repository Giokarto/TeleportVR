using System;
using UnityEngine;
using UnityEngine.XR;

namespace InputDevices
{
    /// <summary>
    /// Abstracts away input devices.
    /// Classes that need to listen for user input can register their delegates here.
    ///
    /// The buttons are named after classic VR controllers, but can be connected to any input device.
    /// </summary>
    public class InputSystem : MonoBehaviour
    {
        public delegate void OnLeftPrimaryButtonPress();
        public static event OnLeftPrimaryButtonPress OnLeftPrimaryButton = delegate{};
        
        public delegate void OnLeftSecondaryButtonPress();
        public static event OnLeftPrimaryButtonPress OnLeftSecondaryButton = delegate{};
        
        public delegate void OnLeftMenuButtonPress();
        public static event OnLeftPrimaryButtonPress OnLeftMenuButton = delegate{};
        
        public delegate void OnRightPrimaryButtonPress();
        public delegate void OnRightSecondaryButtonPress();

        private InputDevice controllerLeft;
        private InputDevice controllerRight;

        private void Start()
        {
            // TODO: find controllers
        }
        
        private void Update()
        {
            bool btn;
            controllerLeft = ControllerInputManager.Instance.controllerLeft[0]; // for now
            if (controllerLeft.TryGetFeatureValue(CommonUsages.menuButton, out btn) && btn)
            {
                OnLeftMenuButton.Invoke();
            }
        }
    }
}