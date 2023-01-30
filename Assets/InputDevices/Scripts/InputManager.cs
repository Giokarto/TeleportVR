using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Widgets;

public class InputManager : Singleton<InputManager>
{
    private List<UnityEngine.XR.InputDevice> controllerLeftList = new List<UnityEngine.XR.InputDevice>();
    private List<UnityEngine.XR.InputDevice> controllerRightList = new List<UnityEngine.XR.InputDevice>();
    private List<UnityEngine.XR.InputDevice> headsetList = new List<UnityEngine.XR.InputDevice>();

    public InputDevice ControllerLeft;
    public InputDevice ControllerRight;
    public InputDevice Headset;

    public bool IsInitialized = false;

    public HandManager handManager;
    public StartCalibrator startCalibrator;

    [SerializeField] VRGestureRecognizer vrGestureRecognizer;

    private bool lastMenuBtn;
    private bool lastGrabLeft;
    private bool lastGrabRight;
    bool nodded, waiting;

    void Awake()
    {
        GetLeftController();
        GetRightController();
        GetHeadset();

        vrGestureRecognizer.Nodded += OnNodded;
        vrGestureRecognizer.HeadShaken += OnHeadShaken;

        IsInitialized = true;
    }

    public bool IsUserActive()
    {
        headsetList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out bool userActive);
        return userActive;
    }

    public bool IsDeviceAvailable(InputDevice device)
    {
        return device != null;
    }

    private bool GetControllerAvailable(bool leftController)
    {
        return leftController ? GetLeftControllerAvailable() : GetRightControllerAvailable();
    }

    public InputDevice GetController(bool leftController)
    {
        return leftController ? controllerLeftList[0] : controllerRightList[0];
    }

    public InputDevice? GetDeviceByName(string name)
    {
        if (name.ToLower().Contains("controller"))
        {
            if (name.ToLower().Contains("left")) return ControllerLeft;
            if (name.ToLower().Contains("right")) return ControllerRight;
        }
        
        else if (name.ToLower().Contains("headset") || name.ToLower().Contains("hmd"))
        {
            return Headset;
        }

        Debug.LogWarning($"Could not find device by name {name}");
        return null;
    }

    /// try to get the left controller, if possible.<!-- return if the controller can be referenced.-->
    public bool GetLeftControllerAvailable()
    {
        if (controllerLeftList.Count == 0)
        {
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left, controllerLeftList);
        }
        return controllerLeftList.Count > 0;
    }

    /// try to get the right controller, if possible.<!-- return if the controller can be referenced.-->
    public bool GetRightControllerAvailable()
    {
        if (controllerRightList.Count == 0)
        {
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, controllerRightList);
        }
        return controllerRightList.Count > 0;
    }

    public bool GetHeadset()
    {
        if (headsetList.Count ==0)
        {
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeadMounted, headsetList);
            if (headsetList.Count > 0)
            {
                Headset = headsetList[0];
            }
        }
            
        return headsetList.Count > 0;
    }

    /// try to get the left controller, if possible.<!-- return if the controller can be referenced.-->
    public bool GetLeftController()
    {
        if (controllerLeftList.Count == 0)
        {
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Left, controllerLeftList);
            if (controllerLeftList.Count > 0 )
            {
                ControllerLeft = controllerLeftList[0];
            }
        }
        return controllerLeftList.Count > 0;
    }

    /// try to get the right controller, if possible.<!-- return if the controller can be referenced.-->
    public bool GetRightController()
    {
        if (controllerRightList.Count == 0)
        {
            UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.Right, controllerRightList);
            if (controllerRightList.Count > 0)
            {
                ControllerRight = controllerRightList[0];
            }
        }
        return controllerRightList.Count > 0;
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

    public bool GetAnyControllerBtnPressed()
    {
        var ret = false;
        var ctrls = new List<bool> { true, false };
        foreach (var left in ctrls)
        {
            if (GetControllerAvailable(left))
            {
                GetController(left).TryGetFeatureValue(CommonUsages.primaryButton, out var btn1);
                GetController(left).TryGetFeatureValue(CommonUsages.secondaryButton, out var btn2);
                ret = ret || btn1 || btn2;
            }
        }
        return ret;
    }

    public Vector2 GetControllerJoystick(bool isLeft)
    {
        if (GetControllerAvailable(isLeft))
        {
            GetController(isLeft).TryGetFeatureValue(CommonUsages.primary2DAxis, out var joystick);
            return joystick;
        }
        return Vector2.zero;
    }

    void OnNodded()
    {
        nodded = true;
        Debug.LogError("Yes");
    }

    void OnHeadShaken()
    {
        Debug.LogError("no");
    }

    IEnumerator WaitForNod()
    {
        Debug.Log("waiting for a nod");
        waiting = true;
        nodded = false;
        yield return new WaitUntil(() => nodded);
        waiting = false;
        if (Training.TutorialSteps.Instance.waitingForNod)
        {
            Debug.Log("user confirmed");
            Training.TutorialSteps.Instance.Next();
        }
    }

    /// <summary>
    /// Handle input from the controllers.
    /// </summary>
    void Update()
    {

        //UnityAnimusClient.Instance.emotion_get();
        //if (StateManager.Instance.currentState == StateManager.States.HUD)
        //    UnityAnimusClient.Instance.EnableMotor(true);
        //else
        //    UnityAnimusClient.Instance.EnableMotor(false);

        // update refs to devices in case tracking was lost
        GetLeftController();
        GetRightController();
        GetHeadset();


        if (!Widgets.WidgetInteraction.settingsAreActive)
        {
            bool btn;
            if (GetLeftController())
            {
                // enable/disable motor
                if (controllerLeftList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out btn) && btn && !lastMenuBtn)
                {
                    //UnityAnimusClient.Instance.ToggleMotor();
                    //startCalibrator.ResetBodyPose(); // re-aling robody model with operator's body every time we enable motion
                    //StateManager.Instance.GoToNextState();
                }
                lastMenuBtn = btn;

                // update the emotion buttons
                //if (controllerLeftList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out btn))
                //{
                //    UnityAnimusClient.Instance.LeftButton1 = btn;
                //    //UnityAnimusClient.Instance.currentEmotion = "shy";
                //}
                //if (controllerLeftList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out btn))
                //{
                //    UnityAnimusClient.Instance.LeftButton2 = btn;
                //    //UnityAnimusClient.Instance.currentEmotion = "hearts";
                //}

                if (Training.TutorialSteps.Instance != null &&
                    StateManager.Instance.currentState == StateManager.States.Training)
                {
                    // check if the arm is grabbing
                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.LEFT_HAND)
                    {
                        if (controllerLeftList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.Next();
                        }

                        if (controllerLeftList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                           btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("grip");
                        }
                    }

                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.RIGHT_HAND)
                    {
                        if (controllerRightList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.Next();
                        }
                        if (controllerRightList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("grip");
                        }
                    }

                    // check left arm
                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.LEFT_ARM)
                    {
                        if (controllerLeftList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("index");
                        }
                    }

                    //// check right arm
                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.RIGHT_ARM)
                    {

                        //if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                        //    btn)
                        //{
                        //    Training.TutorialSteps.Instance.NextStep(praise: true);
                        //}

                        if (controllerRightList[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out btn) &&
                            btn)
                        {
                            Training.TutorialSteps.Instance.CorrectUser("index");
                        }
                    }

                    //if (Training.TutorialSteps.Instance.currentStep == Training.TutorialSteps.TrainingStep.RIGHT_HAND)
                    //{
                    //    if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                    //        btn)
                    //    {
                    //        Training.TutorialSteps.Instance.CorrectUser("grip");
                    //    }
                    //}

                    //if (Training.TutorialSteps.Instance.currentStep == Training.TutorialSteps.TrainingStep.LEFT_HAND)
                    //{
                    //    if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out btn) &&
                    //        btn)
                    //    {
                    //        Training.TutorialSteps.Instance.CorrectUser("grip");
                    //    }
                    //}
                }

                //drive the wheelchair
#if !SENSEGLOVE
                //if (//StateManager.Instance.currentState == StateManager.States.Construct ||
                //    StateManager.Instance.currentState != StateManager.States.HUD)
                //{
                //    Vector2 joystick;
                //    if (controllerLeft[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out joystick))
                //    {
                //        bool wheelchairIsActive = joystick.sqrMagnitude > 0.01f;

                //        // Show that the wheelchair is active in the state manager
                //        WidgetInteraction.SetBodyPartActive(56, wheelchairIsActive);

                //        //if (wheelchairIsActive)
                //        //{
                //        //    if (StateManager.Instance.currentState == StateManager.States.Training &&
                //        //        Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.WHEELCHAIR)
                //        //    {
                //        //        Training.TutorialSteps.Instance.Next();
                //        //    }
                //        //}

                //        float speed = 0.05f;
                //        // move forward or backwards
                //        Debug.Log($"joystick y x: {joystick.y} {joystick.x}");
                //        DifferentialDriveControl.Instance.V_L = speed * joystick.y;
                //        DifferentialDriveControl.Instance.V_R = speed * joystick.y;

                //        //rotate
                //        if (joystick.x > 0)
                //        {
                //            DifferentialDriveControl.Instance.V_R -= 0.5f * speed * joystick.x;
                //        }
                //        else
                //        {
                //            DifferentialDriveControl.Instance.V_L += 0.5f * speed * joystick.x;
                //        }
                //    }
                //}
                //else
                //{
                //    // Show that the wheelchair is active in the state manager
                //    WidgetInteraction.SetBodyPartActive(56, false);
                //}
#endif
            }
            else
            {
                //if (UnityAnimusClient.Instance != null)
                //{
                //    UnityAnimusClient.Instance.LeftButton1 = Input.GetKeyDown(KeyCode.F);
                //    UnityAnimusClient.Instance.LeftButton2 = Input.GetKeyDown(KeyCode.R);
                //}
            }
            if (GetRightController())
            {
                // update the emotion buttons
                //if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out btn))
                //{
                //    UnityAnimusClient.Instance.RightButton1 = btn;
                //}
                //if (controllerRight[0].TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out btn))
                //{
                //    UnityAnimusClient.Instance.RightButton2 = btn;
                //}

                if ( //StateManager.Instance.currentState == StateManager.States.Construct ||
                    StateManager.Instance.currentState == StateManager.States.Training)
                {
                    if (Training.TutorialSteps.Instance.currentState == Training.TutorialSteps.TrainingStep.HEAD)
                    {
                        if (!waiting & Training.TutorialSteps.Instance.waitingForNod) StartCoroutine(WaitForNod());
                        // if (nodded)


                    }
                }
            }
            else
            {
                //if (UnityAnimusClient.Instance != null)
                //{
                //    UnityAnimusClient.Instance.RightButton1 = Input.GetKeyDown(KeyCode.G);
                //    UnityAnimusClient.Instance.RightButton2 = Input.GetKeyDown(KeyCode.T);
                //}
            }
        }
    }

}
