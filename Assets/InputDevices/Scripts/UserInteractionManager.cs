using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Training.Calibration;

public class UserInteractionManager : Singleton<UserInteractionManager>
{
    public enum InputDevice
    {
#if SENSEGLOVE
        SENSE_GLOVE,
#endif
        CONTROLLERS,
        KEYBOARD
    }

    public InputDevice inputDevice = InputDevice.CONTROLLERS;
    public UnityEngine.XR.InputDevice controllerLeft, controllerRight;
    public Widgets.Completion completionWidget;


    private Callbacks<bool> onConfirmCallbacks;

#if SENSEGLOVE
    // SenseGlove params
    public HandCalibrator leftCalibrator, rightCalibrator;
    private const HandCalibrator.Pose confirmationPose = HandCalibrator.Pose.ThumbUp;
#endif
    private const float confirmationPoseError = 0.5f;
    private const float confirmationDwellTime = 3;
    private readonly Timer dwellTimer = new Timer();
    private volatile Coroutine coroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        onConfirmCallbacks = new Callbacks<bool>();
#if SENSEGLOVE
        inputDevice = InputDevice.SENSE_GLOVE;
#else
        inputDevice = InputDevice.CONTROLLERS;
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (coroutine != null)
            {
                dwellTimer.Finish();
            }
            else
            {
                Cleanup();
                onConfirmCallbacks.Call(true);
            }
        }
    }

    public void Confirm(Action<bool> onConfirm, bool left = true, bool once = true)
    {
        switch (inputDevice)
        {
#if SENSEGLOVE
            case InputDevice.SENSE_GLOVE:
                {
                    onConfirmCallbacks.Add(onConfirm, once);

                    if (coroutine == null)
                    {
                        dwellTimer.SetTimer(confirmationDwellTime, () =>
                        {
                            Debug.Log("UserInteractionManager, SenseGlove done");
                            StopCoroutine(coroutine);
                            coroutine = null;
                            Cleanup();
                            onConfirmCallbacks.Call(true);
                        });
                        coroutine = StartCoroutine(SenseGloveConfirm(left));
                    }
                    break;
                }
#endif
            case InputDevice.CONTROLLERS:
                onConfirmCallbacks.Add(onConfirm, once);
                if (coroutine == null)
                {
                    coroutine = StartCoroutine(ControllerConfirm());
                }
                break;
            case InputDevice.KEYBOARD:
                {
                    onConfirmCallbacks.Add(onConfirm, once);
                    if (coroutine == null)
                    {
                        coroutine = StartCoroutine(KeyboardConfirm());
                    }
                    break;
                }
        }
    }

    private void Cleanup()
    {
        completionWidget.Set(0);
        completionWidget.active = false;
    }

#if SENSEGLOVE
    private IEnumerator SenseGloveConfirm(bool left)
    {
        while (true)
        {
            PoseBuffer buffer = new PoseBuffer(bufferSize: 2);
            if (left)
            {
                buffer.AddPose(leftCalibrator.poseValues[(int)confirmationPose]);
                buffer.AddPose(leftCalibrator.GetCurrentPoseValues());
            }
            else
            {
                buffer.AddPose(rightCalibrator.poseValues[(int)confirmationPose]);
                buffer.AddPose(rightCalibrator.GetCurrentPoseValues());
            }

            if (buffer.ComputeError() >= confirmationPoseError)
            {
                dwellTimer.ResetTimer();
            }
            else
            {
                dwellTimer.LetTimePass(Time.deltaTime);
            }
            if (dwellTimer.active)
            {
                completionWidget.progress = dwellTimer.GetFraction();
                completionWidget.Set(dwellTimer.GetFraction(), "hold");
            }
            else
            {
                completionWidget.Set(0);
            }
            yield return new WaitForEndOfFrame();
        }
    }
#endif

    private IEnumerator ControllerConfirm()
    {
        while (true)
        {
            if (InputManager.Instance.GetAnyControllerBtnPressed())
            {
                coroutine = null;
                onConfirmCallbacks.Call(true);
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator KeyboardConfirm()
    {
        while (true)
        {
            if (Input.GetKey(KeyCode.C))
            {
                coroutine = null;
                onConfirmCallbacks.Call(true);
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void OnDestroy()
    {
        StopAllCoroutines();
        dwellTimer.ResetTimer();
    }
}
