using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Training.Calibration;

public class UserInteractionManager : Singleton<UserInteractionManager>
{
    public enum InputDevice
    {
        SENSE_GLOVE,
        CONTROLERS,
        KEYBOARD
    }

    public InputDevice inputDevice = InputDevice.SENSE_GLOVE;
    public UnityEngine.XR.InputDevice controllerLeft, controllerRight;
    public Widgets.Completion completionWidget;
    public HandCalibrator leftCalibrator, rightCalibrator;

    private Callbacks<bool> onConfirmCallbacks;

    // SenseGlove params
    private const HandCalibrator.Pose confirmationPose = HandCalibrator.Pose.ThumbUp;
    private readonly Timer dwellTimer = new Timer();
    private const float confirmationPoseError = 0.5f, confirmationDwellTime = 3;
    private volatile Coroutine coroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        onConfirmCallbacks = new Callbacks<bool>();
        //onAbortCallbacks = new Callbacks<bool>();
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
                onConfirmCallbacks.Call(true);
            }
        }
    }

    public void Confirm(Action<bool> onConfirm, bool left = true, bool once = true)
    {
        switch (inputDevice)
        {
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
                            completionWidget.Set(0);
                            completionWidget.active = false;
                            onConfirmCallbacks.Call(true);
                        });
                        coroutine = StartCoroutine(SenseGloveConfirm(left));
                    }
                    break;
                }
            case InputDevice.CONTROLERS:
                throw new NotImplementedException();
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
            completionWidget.progress = dwellTimer.GetFraction();
            completionWidget.Set(dwellTimer.GetFraction(), "hold");
            yield return new WaitForEndOfFrame();
        }
    }

    private void ControllerConfirm()
    {

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
