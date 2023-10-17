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
        
        private bool controllersAvailable, prevUserActive;

        /// <summary>
        /// TryGetFeatureValue returns true for every frame in which the button is pressed, we want to track only
        /// the first time (such as Input.GetKeyDown), so we need to store the previous state
        /// </summary>
        private bool wasPressedMenu, wasPressedLeftPrimary, wasPressedLeftSecondary, wasPressedRightPrimary, wasPressedRightSecondary;
        
        bool anyButtonCurrentlyPressed = false;
        float lIndexTrigger = 0;
        float rIndexTrigger = 0;
        float rHandTrigger = 0;
        float lHandTrigger = 0;

        
        public static event Action<float, float> OnGripChange = delegate{};
        protected void InvokeGripChange(float left, float right) {OnGripChange?.Invoke(left, right);}
        
        public static event Action<float, float> OnTriggerChange = delegate{};
        protected void InvokeTriggerChange(float left, float right) {OnTriggerChange?.Invoke(left, right);}
        public static Action<float, float>[] StripActions()
        {
            Action<float, float>[] restore = {
                OnGripChange,
                OnTriggerChange
            };
            OnGripChange = delegate{};
            OnTriggerChange = delegate{};
            return restore;
        }

        public static void RestoreActions(Action<float, float>[] restore)
        {
            OnGripChange = restore[0];
            OnTriggerChange = restore[1];
        }

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
            //Debug.Log("Looking for controllers");
            if (!GetControllers())
            {
                Debug.LogWarning("Could not find XR controllers!");
            }
            //Debug.Log($"found controllers");
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
                        //Debug.Log("button pressed");
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

        private float left, right;
        private Vector2 leftVec, rightVec;
        //public override void Update()
        //{
        //    OVRInput.Update();

        //    Debug.Log("VR Input System update");
            
        //    if (!controllersAvailable)
        //    {
        //        if (!GetControllers())
        //        {
        //            return;
        //        }
        //    }

        //    anyButtonCurrentlyPressed = false;
        //    bool btn;

        //    HandleButtonPress(controllerLeft.TryGetFeatureValue(CommonUsages.menuButton, out btn) && btn, 
        //        ref wasPressedMenu, InvokeLeftMenuButton);
        //    HandleButtonPress(controllerLeft.TryGetFeatureValue(CommonUsages.primaryButton, out btn) && btn, 
        //        ref wasPressedLeftPrimary, InvokeLeftPrimaryButton);
        //    HandleButtonPress(controllerLeft.TryGetFeatureValue(CommonUsages.secondaryButton, out btn) && btn, 
        //        ref wasPressedLeftSecondary, InvokeLeftSecondaryButton);
        //    HandleButtonPress(controllerRight.TryGetFeatureValue(CommonUsages.primaryButton, out btn) && btn, 
        //        ref wasPressedRightPrimary, InvokeRightPrimaryButton);
        //    HandleButtonPress(controllerRight.TryGetFeatureValue(CommonUsages.secondaryButton, out btn) && btn, 
        //        ref wasPressedRightSecondary, InvokeRightSecondaryButton);

        //    if (controllerLeft.TryGetFeatureValue(CommonUsages.grip, out left))
        //    {
        //        InvokeGripChange(left, right);
        //        Debug.Log("Grip change");
        //    }
        //    if (controllerRight.TryGetFeatureValue(CommonUsages.grip, out right)) InvokeGripChange(left,right);

        //    if ((controllerLeft.TryGetFeatureValue(CommonUsages.trigger, out left) && left>0) ||
        //        (controllerRight.TryGetFeatureValue(CommonUsages.trigger, out right) && right>0))
        //    {
        //        InvokeTriggerChange(left, right);
        //    }
            
        //    if (controllerLeft.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftVec))
        //    {
        //        joystickY[inputSystemOrder] = leftVec.y;
        //    }
        //    if (controllerRight.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightVec))
        //    {
        //        joystickX[inputSystemOrder] = rightVec.x;
        //    }

        //    if (anyButtonCurrentlyPressed)
        //    {
        //        InvokeAnyButton();
        //        Debug.Log("Any button invoke");
        //    }

        //    DisplayInputs();
        //}

        public override void Update()
        {
            OVRInput.Update();

            //Debug.Log("VR Input System update");

            if (!controllersAvailable)
            {
                if (!GetControllers())
                {
                    return;
                }
            }

            anyButtonCurrentlyPressed = false;
            //bool btn;

            HandleButtonPress(OVRInput.GetDown(OVRInput.RawButton.Start),
                ref wasPressedMenu, InvokeLeftMenuButton);
            HandleButtonPress(OVRInput.Get(OVRInput.RawButton.X),
                ref wasPressedLeftPrimary, InvokeLeftPrimaryButton);
            HandleButtonPress(OVRInput.Get(OVRInput.RawButton.Y),
                ref wasPressedLeftSecondary, InvokeLeftSecondaryButton);
            HandleButtonPress(OVRInput.Get(OVRInput.RawButton.A),
                ref wasPressedRightPrimary, InvokeRightPrimaryButton);
            HandleButtonPress(OVRInput.Get(OVRInput.RawButton.B),
                ref wasPressedRightSecondary, InvokeRightSecondaryButton);

            //HandleButtonPress(controllerRight.TryGetFeatureValue(CommonUsages.secondaryButton, out btn) && btn,
            //   ref wasPressedRightSecondary, InvokeRightSecondaryButton);



            //if (controllerLeft.TryGetFeatureValue(CommonUsages.grip, out left))
            //{
            //    InvokeGripChange(left, right);
            //    //Debug.Log("Grip change");
            //}
            //if (controllerRight.TryGetFeatureValue(CommonUsages.grip, out right)) InvokeGripChange(left, right);

            //if ((controllerLeft.TryGetFeatureValue(CommonUsages.trigger, out left) && left > 0) ||
            //    (controllerRight.TryGetFeatureValue(CommonUsages.trigger, out right) && right > 0))
            //{
            //    InvokeTriggerChange(left, right);
            //}

            rIndexTrigger = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
            lIndexTrigger = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
            if (rIndexTrigger > 0 || lIndexTrigger > 0)
            {

                //Debug.Log($"Invoking trigger change: l: {lIndexTrigger} r: {rIndexTrigger}");
                InvokeTriggerChange(lIndexTrigger, rIndexTrigger);
            }


            leftVec = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
            rightVec = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);

            joystickY[inputSystemOrder] = leftVec.y;
            joystickX[inputSystemOrder] = rightVec.x;

            //if (controllerLeft.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftVec))
            //{
            //    joystickY[inputSystemOrder] = leftVec.y;
            //}
            //if (controllerRight.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightVec))
            //{
            //    joystickX[inputSystemOrder] = rightVec.x;
            //}

            if (anyButtonCurrentlyPressed)
            {
                InvokeAnyButton();
                //Debug.Log("Any button invoke");
            }

            //DisplayInputs();
        }

        private void DisplayInputs()
        {
            string message = "";
            ///                     RIGHT CONTROLLER
            ///Display pressed buttons on ui to determine if input code functions correctly
            //if the right trigger is pressed, then the ui should display this respectively...
            if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.35f)
            {
                message += "Held";
            }
            else
            {
                message += "< Released >";
            }
            //Right Hand Trigger...
            if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.35f)
            {
                message += "Held";
            }
            else
            {
                message += "< Released >";
            }
            //A Button...
            if (OVRInput.Get(OVRInput.Button.One))
            {
                message += "Held";
            }
            else
            {
                message += "< Released >";
            }
            //B Button..
            if (OVRInput.Get(OVRInput.Button.Two))
            {
                message += "Held";
            }
            else
            {
                message += "< Released >";
            }
            //Thumbstick Button..
            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstick))
            {
                message += "Held";
            }
            else
            {
                message += "< Released >";
            }

            //thumbstick direction detection
            if ((OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y > 0.1) || (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y < 0.1))
            {
                message += OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y.ToString();
            }
            else
            {
                message += "< Released >";
            }

            //thumbstick direction detection
            if ((OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x > 0.1) || (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x < 0.1))
            {
                message += OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x.ToString();
            }
            else
            {
                message += "< Released >";
            }



            /////
            /////                     LEFT CONTROLLER
            /////
            ////if the right trigger is pressed, then the ui should display this respectively...
            //if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.35f)
            //{
            //    input_L_IndTrigger.text = "Held";
            //}
            //else
            //{
            //    input_L_IndTrigger.text = "< Released >";
            //}
            ////Right Hand Trigger...
            //if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.35f)
            //{
            //    input_L_HandTrigger.text = "Held";
            //}
            //else
            //{
            //    input_L_HandTrigger.text = "< Released >";
            //}
            ////A Button...
            //if (OVRInput.Get(OVRInput.Button.Three))
            //{
            //    input_L_X.text = "Held";
            //}
            //else
            //{
            //    input_L_X.text = "< Released >";
            //}
            ////B Button..
            //if (OVRInput.Get(OVRInput.Button.Four))
            //{
            //    input_L_Y.text = "Held";
            //}
            //else
            //{
            //    input_L_Y.text = "< Released >";
            //}
            ////Thumbstick Button..
            //if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick))
            //{
            //    input_L_ThumbStick.text = "Held";
            //}
            //else
            //{
            //    input_L_ThumbStick.text = "< Released >";
            //}

            ////thumbstick direction detection
            //if ((OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y > 0.1) || (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y < 0.1))
            //{
            //    input_L_ThumbY.text = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y.ToString();
            //}
            //else
            //{
            //    input_L_ThumbY.text = "< Released >";
            //}

            ////thumbstick direction detection
            //if ((OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x > 0.1) || (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x < 0.1))
            //{
            //    input_L_ThumbX.text = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x.ToString();
            //}
            //else
            //{
            //    input_L_ThumbX.text = "< Released >";
            //}
            Debug.Log(message);
        }

        private static bool userPresentInHeadset, prevUserPresentInHeadset = true;
        public static bool IsUserActive()
        {
            // #if UNITY_EDITOR
            //             return true;
            // #endif            
            //if (!headset.TryGetFeatureValue(CommonUsages.userPresence, out bool  userActive))
            //    return false;

            //headset.TryGetFeatureValue(CommonUsages.userPresence, out bool userActive);
            //return userActive;

            return true;
        }

        public static bool UserActivated()
        {
            
            var ret =  !prevUserPresentInHeadset && IsUserActive();
            //Debug.Log($"User activated: {ret}");
            prevUserPresentInHeadset = IsUserActive();
            return ret;

        }

        public static bool UserDeactivated()
        {
            var ret =  prevUserPresentInHeadset && !IsUserActive();
            prevUserPresentInHeadset = IsUserActive();
            //Debug.Log($"User deactivated: {ret}");
            return ret;
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