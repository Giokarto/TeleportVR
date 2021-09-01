using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Training.Calibration
{
    public class CalibrationVolume : MonoBehaviour
    {
        [Tooltip("Tag the colliding object neets to have, in order to trigger calibration")]
        public string requiredTag;
        [Tooltip("Array of TrainingSteps, during which to trigger the calibration")]
        public TutorialSteps.TrainingStep[] requiredTrainingSteps;
        [Tooltip("Array of Steps, during which to trigger the calibration")]
        public HandCalibrator.Step[] requiredCalibrationSteps;
        [Tooltip("Calibator the calibration will be triggered for")]
        public HandCalibrator calibrator;
        [Tooltip("Renderer of the current object")]
        public new MeshRenderer renderer;
        public int collisionLayerIndex = 7;

        private new bool enabled
        {
            get
            {
                return requiredTrainingSteps.Contains(TutorialSteps.Instance.currentState)
                  && requiredCalibrationSteps.Contains(calibrator.currentStep);
            }
        }
        private bool lastEnabled = false;
        private bool coroutineRunning = false;

        private bool colliding = false;

#if SENSEGLOVE
        // Update is called once per frame
        void Update()
        {
            // only render the volume, if we're in the right calibration step(s)
            renderer.enabled = enabled;
            //if (enabled && !lastEnabled)
            //{
            //    OnTriggerEnter(null);
            //}
            lastEnabled = enabled;
        }

        private void OnTriggerEnter(Collider other)
        {
            // if other == null skip the tag comarison
            if (!enabled || !other.CompareTag(requiredTag))
            {
                return;
            }
            Debug.Log("on trigger enter");
            colliding = true;
            if (!coroutineRunning)
            {
                coroutineRunning = true;
                StartCoroutine(CalibrateOnAudioDone());
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!enabled || !other.CompareTag(requiredTag))
            {
                return;
            }
            colliding = true;
        }

        private IEnumerator CalibrateOnAudioDone()
        {
            while (TutorialSteps.Instance.audioManager.IsAudioPlaying())
            {
                yield return new WaitForSeconds(0.5f);
            }
            if (enabled && colliding)
            {
                Debug.Log("starting calibration");
                calibrator.StartCalibration();
            }
            coroutineRunning = false;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!enabled || !other.CompareTag(requiredTag))
            {
                return;
            }
            colliding = false;
            calibrator.PauseCalibration();
            TutorialSteps.Instance.audioManager.StopAudioClips();
        }
#endif
    }
}