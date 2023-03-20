using System;
using System.Collections;
using UnityEngine;
using InputSystem = InputDevices.InputSystem;

namespace RobodyControl
{
    /// <summary>
    /// This script moves the associated transform such that the reference eye transform matches the goal. 
    /// Calibration is done <see cref="calibrationTime"/> number of seconds after the game has started, to insure the operator
    /// has enough time to settle.
    /// Used to move the initial camera position to match with Roboy's head.
    /// </summary>
    public class StartCalibrator : MonoBehaviour
    {
        [Tooltip("Operator eye transform, used as calibration reference")]
        public Transform eye;

        [Tooltip("Goal eye transform after calibration")]
        public Transform goal;

        [Tooltip("Number of seconds after game start, calibration should occur (seconds)")]
        public float calibrationTime = 2;

        void Start()
        {
            StartCoroutine(WaitAndCalibrate(calibrationTime));
        }

        private IEnumerator WaitAndCalibrate(float waitTime, bool withRotation = true)
        {
            yield return new WaitForSeconds(waitTime);
            // Change transfrom position
            Vector3 move = goal.position - eye.position;
            transform.position += move;

            if (withRotation)
            {
                // Change transform orientation
                var rotate = new Vector3(0f, -eye.rotation.eulerAngles.y, 0f);
                transform.Rotate(rotate);
                
                // Rotation changes world space position of the goal (Roboy's head gets straight) -> we need to move again after the head moves!
                // Empirically, rotation of the head takes about 10 frames
                // Solution: just repeat the calibration for the next 1 second (it is smoother than waiting 1 second and then recalibrating)
                var now = DateTime.Now;
                while (DateTime.Now < now + TimeSpan.FromSeconds(1))
                {
                    yield return WaitAndCalibrate(0, false);
                }
            }
        }

        private void Calibrate()
        {
            StartCoroutine(WaitAndCalibrate(0));
        }

        private void OnEnable()
        {
            InputSystem.OnRightSecondaryButton += Calibrate;
        }

        private void OnDisable()
        {
            InputSystem.OnRightSecondaryButton -= Calibrate;
        }
    }
}