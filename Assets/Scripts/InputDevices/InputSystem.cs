using System;
using System.Collections.Generic;
using UnityEngine;

namespace InputDevices
{
    /// <summary>
    /// Abstracts away input devices.
    /// Classes that need to listen for user input can register their delegates here.
    ///
    /// The buttons are named after classic VR controllers, but can be connected to any input device.
    ///
    /// Derived classes should check for input from a specific input device and invoke the delegates
    /// in Update. Derived classes can also define own delegates. See <see cref="VRControllers.VRControllerInputSystem"/>
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

        // x-values from all input systems
        protected static float[] joystickX;
        public static float GetJoystickX()
        {
            return SelectAbsoluteMaximum(joystickX);
        }

        // y-values from all input systems
        protected static float[] joystickY;
        public static float GetJoystickY()
        {
            return SelectAbsoluteMaximum(joystickY);
        }

        private static float SelectAbsoluteMaximum(float[] array)
        {
            float max = 0;
            foreach (var i in array)
            {
                if (Math.Abs(i) > Math.Abs(max))
                {
                    max = i;
                }
            }
            return max;
        }

        /// <summary>
        /// Derived classes should check input here and invoke the delegates.
        /// </summary>
        public abstract void Update();

        protected int inputSystemOrder;
        private static List<InputSystem> registeredInputSystems = new List<InputSystem>();
        public void Awake()
        {
            this.inputSystemOrder = registeredInputSystems.Count;
            InputSystem.registeredInputSystems.Add(this);
            Array.Resize(ref joystickX, registeredInputSystems.Count);
            Array.Resize(ref joystickY, registeredInputSystems.Count);
        }
    }
}