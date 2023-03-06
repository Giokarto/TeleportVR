using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;

namespace InputDevices.VRControllers
{
    /// <summary>
    /// Input system for control with classic headset and controllers.
    /// </summary>
    public class VRControllerInputSystem: InputSystem
    {
        public static InputDevice controllerLeft { get; private set; }
        public static InputDevice controllerRight { get; private set; }
        public static InputDevice headset { get; private set; }
        
        private bool controllersAvailable;

        /// <summary>
        /// TryGetFeatureValue returns true for every frame in which the button is pressed, we want to track only
        /// the first time (such as Input.GetKeyDown), so we need to store the previous state
        /// </summary>
        private bool wasPressedMenu, wasPressedLeftPrimary, wasPressedLeftSecondary, wasPressedRightPrimary, wasPressedRightSecondary;
        
        bool anyButtonCurrentlyPressed = false;
        
        public static event Action<float, float> OnGripChange = delegate{};
        protected void InvokeGripChange(float left, float right) {OnGripChange?.Invoke(left, right);}
        
        public static event Action<float, float> OnTriggerChange = delegate{};
        protected void InvokeTriggerChange(float left, float right) {OnTriggerChange?.Invoke(left, right);}

        /// <summary>
        /// variable used in GetControllers, moved outside to the class to not create garbage on the heap in each update
        /// </summary>
        private List<InputDevice> foundControllers = new List<InputDevice>();
        private bool GetControllers()
        {
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, foundControllers);
            if (foundControllers.Count > 0)
            {
                controllerLeft = foundControllers[0];
            }
            
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, foundControllers);
            if (foundControllers.Count > 0)
            {
                controllerRight = foundControllers[0];
            }
            
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, foundControllers);
            if (foundControllers.Count > 0)
            {
                headset = foundControllers[0];
            }

            controllersAvailable = controllerLeft != null && controllerRight != null && headset != null;
            return controllersAvailable;
        }

        private void Start()
        {
            if (!GetControllers())
            {
                Debug.LogWarning("Could not find XR controllers!");
            }
        }

        /// <summary>
        /// Checks if button is pressed, invokes the delegate and sets state to remember later.
        /// Used below to get rid of copy-pasted conditions.
        /// </summary>
        /// <param name="isPressed">is the button currently pressed?</param>
        /// <param name="previouslyPressed">ref to bool of previous state (ref to change the content)</param>
        /// <param name="actionToInvoke"></param>
        private void HandleButtonPress(bool isPressed, ref bool previouslyPressed, Action actionToInvoke)
        {
            if (isPressed)
            {
                if (!previouslyPressed)
                {
                    try
                    {
                        actionToInvoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                previouslyPressed = true;
                anyButtonCurrentlyPressed = true;
            }
            else
            {
                previouslyPressed = false;
            }
        }
        
        public override void Update()
        {
            if (!controllersAvailable)
            {
                if (!GetControllers())
                {
                    return;
                }
            }

            anyButtonCurrentlyPressed = false;
            bool btn;

            HandleButtonPress(controllerLeft.TryGetFeatureValue(CommonUsages.menuButton, out btn) && btn, 
                ref wasPressedMenu, InvokeLeftMenuButton);
            HandleButtonPress(controllerLeft.TryGetFeatureValue(CommonUsages.primaryButton, out btn) && btn, 
                ref wasPressedLeftPrimary, InvokeLeftPrimaryButton);
            HandleButtonPress(controllerLeft.TryGetFeatureValue(CommonUsages.secondaryButton, out btn) && btn, 
                ref wasPressedLeftSecondary, InvokeLeftSecondaryButton);
            HandleButtonPress(controllerRight.TryGetFeatureValue(CommonUsages.primaryButton, out btn) && btn, 
                ref wasPressedRightPrimary, InvokeRightPrimaryButton);
            HandleButtonPress(controllerRight.TryGetFeatureValue(CommonUsages.secondaryButton, out btn) && btn, 
                ref wasPressedRightSecondary, InvokeRightSecondaryButton);

            float left, right = 0;
            if ((controllerLeft.TryGetFeatureValue(CommonUsages.grip, out left) && left>0) ||
                (controllerRight.TryGetFeatureValue(CommonUsages.grip, out right) && right>0))
            {
                InvokeGripChange(left, right);
            }
            
            if ((controllerLeft.TryGetFeatureValue(CommonUsages.trigger, out left) && left>0) ||
                (controllerRight.TryGetFeatureValue(CommonUsages.trigger, out right) && right>0))
            {
                InvokeTriggerChange(left, right);
            }

            if (anyButtonCurrentlyPressed)
            {
                InvokeAnyButton();
            }
        }

        private static bool userPresentInHeadset;
        public static bool IsUserActive()
        {
            return headset.TryGetFeatureValue(CommonUsages.userPresence, out userPresentInHeadset) && userPresentInHeadset;
        }

        public static InputDevice GetDeviceByName(string name)
        {
            if (name.Contains("left"))
            {
                return controllerLeft;
            }

            if (name.Contains("right"))
            {
                return controllerRight;
            }

            if (name.Contains("head"))
            {
                return headset;
            }
            
            else
            {
                throw new NullReferenceException($"Device with name {name} not found!");
            }
        }
    }
}