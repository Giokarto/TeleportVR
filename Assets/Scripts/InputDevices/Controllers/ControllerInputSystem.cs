using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;

namespace InputDevices.Controllers
{
    /// <summary>
    /// Input system for control with classic headset and controllers.
    /// </summary>
    public class ControllerInputSystem: InputSystem
    {
        private InputDevice controllerLeft;
        private InputDevice controllerRight;
        
        private bool controllersAvailable;
        
        public static event Action<float, float> OnGripChange = delegate{};
        protected void InvokeGripChange(float left, float right) {OnGripChange?.Invoke(left, right);}
        
        public static event Action<float, float> OnTriggerChange = delegate{};
        protected void InvokeTriggerChange(float left, float right) {OnTriggerChange?.Invoke(left, right);}
        
        private bool GetControllers()
        {
            var foundControllersLeft = new List<InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, foundControllersLeft);
            if (foundControllersLeft.Count > 0)
            {
                controllerLeft = foundControllersLeft[0];
            }
            
            var foundControllersRight = new List<InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, foundControllersRight);
            if (foundControllersRight.Count > 0)
            {
                controllerRight = foundControllersRight[0];
            }
            
            controllersAvailable = foundControllersLeft.Count > 0 && foundControllersRight.Count > 0;
            return controllersAvailable;
        }

        private void Start()
        {
            if (!GetControllers())
            {
                Debug.Log("Could not find XR controllers!");
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
                else
                {
                    Debug.Log("Found XR Controllers");
                }
            }
            else
            {
                bool btn;
                bool anyButton = false;
                if (controllerLeft.TryGetFeatureValue(CommonUsages.menuButton, out btn) && btn)
                {
                    InvokeLeftMenuButton();
                }
                if (controllerLeft.TryGetFeatureValue(CommonUsages.primaryButton, out btn) && btn)
                {
                    anyButton = true;
                    InvokeLeftPrimaryButton();
                }
                if (controllerLeft.TryGetFeatureValue(CommonUsages.secondaryButton, out btn) && btn)
                {
                    anyButton = true;
                    InvokeLeftSecondaryButton();
                }
                if (controllerRight.TryGetFeatureValue(CommonUsages.primaryButton, out btn) && btn)
                {
                    anyButton = true;
                    InvokeRightPrimaryButton();
                }
                if (controllerRight.TryGetFeatureValue(CommonUsages.secondaryButton, out btn) && btn)
                {
                    anyButton = true;
                    InvokeRightSecondaryButton();
                }

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

                if (anyButton)
                {
                    InvokeAnyButton();
                }
            }
        }

    }
}