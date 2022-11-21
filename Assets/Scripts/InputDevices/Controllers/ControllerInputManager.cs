using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace InputDevices.Controllers
{
    public class ControllerInputManager: Singleton<ControllerInputManager>
    {
        private InputDevice controllerLeft;
        private InputDevice controllerRight;
        private UserInteractionEventSystem uiEventSystem;
        private bool controllersAvailable;
        
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
            uiEventSystem = UserInteractionEventSystem.Instance;
            
            if (!GetControllers())
            {
                Debug.LogError("Could not find XR controllers!");
            }
        }
        
        /// <summary>
        /// Handle input from the controllers.
        /// </summary>
        void Update()
        {
            bool btn;
            if (!controllersAvailable)
            {
                if (!GetControllers())
                {
                    return;
                }
            }
            
            if (controllerLeft.TryGetFeatureValue(CommonUsages.menuButton, out btn) && btn)
            {
                uiEventSystem.OpenMenu();
            }
            
            if (controllerLeft.TryGetFeatureValue(CommonUsages.primaryButton, out btn) && btn)
            {
                uiEventSystem.SendEmotion("shy");
            }
            
            if (controllerLeft.TryGetFeatureValue(CommonUsages.secondaryButton, out btn) && btn)
            {
                uiEventSystem.SendEmotion("hearts");
            }
            
            if (controllerLeft.TryGetFeatureValue(CommonUsages.gripButton, out btn) && btn)
            {
                uiEventSystem.ChangeGrip(500, 500); // TODO better
            }


            bool tutorial = false;
            if (tutorial)
            {
                if (Training.TutorialSteps.Instance != null &&
                    StateManager.Instance.currentState == StateManager.States.Training)
                {
                    // check if the arm is grabbing
                    if (Training.TutorialSteps.Instance.currentState ==
                        Training.TutorialSteps.TrainingStep.LEFT_HAND)
                    {
                        if (controllerLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.Next();
                        }

                        if (controllerLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("grip");
                        }
                    }

                    if (Training.TutorialSteps.Instance.currentState ==
                        Training.TutorialSteps.TrainingStep.RIGHT_HAND)
                    {
                        if (controllerRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.Next();
                        }

                        if (controllerRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("grip");
                        }
                    }

                    // check left arm
                    if (Training.TutorialSteps.Instance.currentState ==
                        Training.TutorialSteps.TrainingStep.LEFT_ARM)
                    {
                        if (controllerLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("index");
                        }
                    }

                    //// check right arm
                    if (Training.TutorialSteps.Instance.currentState ==
                        Training.TutorialSteps.TrainingStep.RIGHT_ARM)
                    {
                        if (controllerRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("index");
                        }
                    }

                }

            }
        }

    }
}