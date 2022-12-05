using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OperatorUserInterface;
using UnityEngine.XR;

namespace InputDevices
{
    [Obsolete("Use ControllerInputSystem instead.")]
    public class ControllerInputManager : Singleton<ControllerInputManager>
    {
        public List<UnityEngine.XR.InputDevice> controllerLeft = new List<UnityEngine.XR.InputDevice>();
        public List<UnityEngine.XR.InputDevice> controllerRight = new List<UnityEngine.XR.InputDevice>();
        public HandManager handManager;

        [SerializeField] VRGestureRecognizer vrGestureRecognizer;

        private bool lastMenuBtn;
        private bool lastGrabLeft;
        private bool lastGrabRight;
        bool nodded, waiting;

        void Start()
        {
            GetLeftController();
            GetRightController();
        }

        private bool GetControllerAvailable(bool leftController)
        {
            return leftController ? GetLeftControllerAvailable() : GetRightControllerAvailable();
        }

        public InputDevice GetController(bool leftController)
        {
            return leftController ? controllerLeft[0] : controllerRight[0];
        }

        /// try to get the left controller, if possible.<!-- return if the controller can be referenced.-->
        public bool GetLeftControllerAvailable()
        {
            if (controllerLeft.Count == 0)
            {
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left,
                    controllerLeft);
            }

            return controllerLeft.Count > 0;
        }

        /// try to get the right controller, if possible.<!-- return if the controller can be referenced.-->
        public bool GetRightControllerAvailable()
        {
            if (controllerRight.Count == 0)
            {
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right,
                    controllerRight);
            }

            return controllerRight.Count > 0;
        }

        /// try to get the left controller, if possible.<!-- return if the controller can be referenced.-->
        public bool GetLeftController()
        {
            if (controllerLeft.Count == 0)
            {
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
                    UnityEngine.XR.InputDeviceCharacteristics.Left, controllerLeft);
            }

            return controllerLeft.Count > 0;
        }

        /// try to get the right controller, if possible.<!-- return if the controller can be referenced.-->
        public bool GetRightController()
        {
            if (controllerRight.Count == 0)
            {
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
                    UnityEngine.XR.InputDeviceCharacteristics.Right, controllerRight);
            }

            return controllerRight.Count > 0;
        }

        public bool GetControllerBtn(InputFeatureUsage<bool> inputFeature, bool leftController)
        {
            if (GetControllerAvailable(leftController))
            {
                if (GetController(leftController).TryGetFeatureValue(inputFeature, out var btn))
                {
                    return btn;
                }
            }

            return false;
        }

        /// <summary>
        /// Handle input from the controllers.
        /// </summary>
        void Update()
        {
            if (!DeprecatedWidgets.WidgetInteraction.settingsAreActive)
            {
                bool btn;
                if (GetLeftController())
                {
                }
            }
        }

    }
}