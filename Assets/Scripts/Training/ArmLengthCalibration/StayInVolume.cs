using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Training.Calibration.ArmLength
{
    public class StayInVolume : MonoBehaviour
    {
        //[Tooltip("Completion widget in HUD")]
        //public DeprecatedWidgets.Completion completionWidget;
        [Tooltip("Required tag for the colliders to trigger waiting")]
        public string requiredTag;
        [Tooltip("Time to wait for until triggering Callbacks")]
        public float waitTime;

        private Timer completionTimer = new Timer();
        private Callbacks<bool> onDoneCallbacks = new Callbacks<bool>();
        [SerializeField] private bool started = false;
        [SerializeField] private bool waiting = false;


        // Update is called once per frame
        void Update()
        {
            if (started && waiting)
            {
                completionTimer.LetTimePass(Time.deltaTime);
                //completionWidget.Set(completionTimer.GetFraction(), "hold");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(requiredTag) || !started)
            {
                return;
            }

            completionTimer.SetTimer(waitTime, () =>
            {
                Reset();
                onDoneCallbacks.Call(true);
            });
            waiting = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(requiredTag) && waiting)
            {
                Reset();
            }
        }

        private void Reset()
        {
            //completionWidget.Set(0);
            completionTimer.ResetTimer();
            waiting = false;
        }

        public void StartWaiting() => started = true;

        public void StopWaiting()
        {
            started = false;
            Reset();
        }

        public void OnDone(System.Action<bool> onDone, bool once = true) => onDoneCallbacks.Add(onDone, once);
    }
}
