using System;
using InputDevices.VRControllers;
using UnityEngine;
using UnityEngine.XR;

namespace InputDevices
{
    /// <summary>
    /// Abstracts away input devices.
    /// Classes that need to listen for user input can register their delegates here.
    ///
    /// The buttons are named after classic VR controllers, but can be connected to any input device.
    ///
    /// Derived classes should check for input from a specific input device and invoke the delegates
    /// in Update. Derived classes can also define own delegates. See <see cref="VRControllerInputSystem"/>
    /// </summary>
    public abstract class InputSystem : MonoBehaviour
    {
        [SerializeField] VRGestureRecognizer vrGestureRecognizer;
        
        public static event Action OnLeftPrimaryButton = delegate{};
        protected void InvokeLeftPrimaryButton() {OnLeftPrimaryButton?.Invoke();}
        
        public static event Action OnLeftSecondaryButton = delegate{};
        protected void InvokeLeftSecondaryButton() {OnLeftSecondaryButton?.Invoke();}
        
        public static event Action OnLeftMenuButton = delegate{};
        protected void InvokeLeftMenuButton() {OnLeftMenuButton?.Invoke();}
        
        public static event Action OnRightPrimaryButton = delegate{};
        protected void InvokeRightPrimaryButton() {OnRightPrimaryButton?.Invoke();}
        
        public static event Action OnRightSecondaryButton = delegate{};
        protected void InvokeRightSecondaryButton() {OnRightSecondaryButton?.Invoke();}
        
        public static event Action OnAnyButton = delegate{};
        protected void InvokeAnyButton() {OnAnyButton?.Invoke();}

        public static Action[] StripActions()
        {
            Action[] restore = {
                OnAnyButton,
                OnLeftMenuButton,
                OnLeftPrimaryButton,
                OnLeftSecondaryButton,
                OnRightPrimaryButton,
                OnRightSecondaryButton
            };
            OnAnyButton = delegate{};
            OnLeftMenuButton = delegate{};
            OnLeftPrimaryButton = delegate{};
            OnLeftSecondaryButton = delegate{};
            OnRightPrimaryButton = delegate{};
            OnRightSecondaryButton = delegate{};
            return restore;
        }

        public static void RestoreActions(Action[] restore)
        {
            OnAnyButton = restore[0];
            OnLeftMenuButton = restore[1];
            OnLeftPrimaryButton = restore[2];
            OnLeftSecondaryButton = restore[3];
            OnRightPrimaryButton = restore[4];
            OnRightSecondaryButton = restore[5];
        }
        
        
        // TODO: joystick to drive the wheelchair

        /// <summary>
        /// Derived classes should check input here and invoke the delegates.
        /// </summary>
        public abstract void Update();
    }
}