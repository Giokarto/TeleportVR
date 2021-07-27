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

    public InputDevice inputDevice;
    public UnityEngine.XR.InputDevice controllerLeft, controllerRight;
    public Widgets.Completion completionWidget;
    public HandCalibrator leftCalibrator, rightCalibrator;

    private Callbacks<bool> onConfirmCallbacks, onAbortCallbacks;

    // SenseGlove params
    private const HandCalibrator.Pose confirmationPose = HandCalibrator.Pose.ThumbUp;
    private readonly Timer dwellTimer = new Timer();
    private const float confirmationPoseError = 0.5f, confirmationDwellTime = 3;
    private Coroutine senseGloveConfirmRoutine;

    // Start is called before the first frame update
    void Start()
    {
        onConfirmCallbacks = new Callbacks<bool>();
        onAbortCallbacks = new Callbacks<bool>();
    }

    public void Confirm(Action<bool> onConfirm, bool once = true)
    {
        switch (inputDevice)
        {
            case InputDevice.SENSE_GLOVE:
                if (senseGloveConfirmRoutine != null)
                {
                    onConfirmCallbacks.Add(onConfirm, once);
                    break;
                }
                dwellTimer.SetTimer(confirmationDwellTime, () =>
                {
                    StopCoroutine(senseGloveConfirmRoutine);
                    completionWidget.active = false;
                    onConfirmCallbacks.Call(true);
                });
                completionWidget.active = true;
                completionWidget.text = "hold";
                StartCoroutine(SenseGloveConfirm());
                break;
            case InputDevice.CONTROLERS:
                throw new NotImplementedException();
            case InputDevice.KEYBOARD:
                throw new NotImplementedException();
        }
    }

    private IEnumerator SenseGloveConfirm()
    {
        PoseBuffer buffer = new PoseBuffer(bufferSize: 2);
        buffer.AddPose(leftCalibrator.poseValues[(int)confirmationPose]);
        buffer.AddPose(leftCalibrator.GetCurrentPoseValues());
        float leftError = buffer.ComputeError();

        buffer.Clear();
        buffer.AddPose(rightCalibrator.poseValues[(int)confirmationPose]);
        buffer.AddPose(rightCalibrator.GetCurrentPoseValues());
        float rightError = buffer.ComputeError();

        if (Mathf.Min(leftError, rightError) >= confirmationPoseError)
        {
            dwellTimer.ResetTimer();
        }
        else
        {
            dwellTimer.LetTimePass(Time.deltaTime);
        }
        completionWidget.progress = dwellTimer.GetFraction();
        yield return new WaitForEndOfFrame();
    }

    private void ControllerConfirm()
    {

    }

    // TODO: Not yet implemented
    public void Abort(Action onAbort, bool once = true)
    {

    }

    public void OnDestroy()
    {
        StopAllCoroutines();
        dwellTimer.ResetTimer();
    }
}
