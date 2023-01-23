using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script moves the associated transfrom such that the reference eye transform matches the goal. 
/// Calibration is done calibrationTime number of seconds after the game has started, to insure the operator
/// has enough time to settle.
/// </summary>
public class StartCalibrator : MonoBehaviour
{
    [Tooltip("Operator eye transfrom, used as calibration reference")]
    public Transform eye;
    [Tooltip("Goal eye transform after calibration")]
    public Transform goal;

    [Tooltip("Number of seconds after game start, calibration should occurr (seconds)")]
    public float calibrationTime = 2;
    private bool calibrated = false;

    void Start()
    {
        StartCoroutine(WaitAndCalibrate(calibrationTime));
    }

    private IEnumerator WaitAndCalibrate(float waitTime, bool withRotation=false)
    {
        yield return new WaitForSeconds(waitTime);
        // Change transfrom position
        Vector3 move = goal.position - eye.position;
        transform.position += move;

        if (withRotation)
        {
            // Change transform orientation
            var rotate = new Vector3(0f, - eye.rotation.eulerAngles.y, 0f); // goal.rotation.eulerAngles.y
            var newRot = transform.rotation.eulerAngles + rotate;
            transform.rotation = Quaternion.Euler(newRot);
        }
        calibrated = true;
        Debug.Log($"Calibration done, moved: {move}");
    }

    public void ResetBodyPose()
    {
        StartCoroutine(WaitAndCalibrate(0, true));
    }

    private void Update()
    {
        bool btn;
        if (InputManager.Instance.GetRightController())
        {
            InputManager.Instance.ControllerRight.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out btn);
            if (btn)
            {
                Debug.Log("Manual position calibration triggered.");
                StartCoroutine(WaitAndCalibrate(0, true));
            }
        }
        
    }

}
