using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace RudderPedals
{
    public class PresenceDetector : Singleton<PresenceDetector>
    {
        [System.Serializable]
        public class TrackerSwitcher
        {
            [Tooltip("CopyTransform script at SenseGlove root")]
            public JointTransfer.CopyTransfrom copyTransform;
            [Tooltip("XR input for the associated hand")]
            public Transform xrController;
            [Tooltip("Ghost hand, shown when paused")]
            public GameObject ghostHand;

            private Transform oldParent;
            private bool usingXR = false;

            internal void SwitchControllers()
            {
                if (usingXR)
                {
                    copyTransform.gameObject.transform.SetParent(oldParent);
                    copyTransform.position = true;
                    copyTransform.rotation = true;
                    usingXR = false;
                }
                else
                {
                    copyTransform.position = false;
                    copyTransform.rotation = false;
                    oldParent = copyTransform.gameObject.transform.parent;
                    copyTransform.gameObject.transform.SetParent(xrController, true);

                    usingXR = true;
                }
            }
        }

        public bool isPaused = false;

        // if the presence detector is allowed to pause / unpause the game
        public bool canPause
        {
            get { return _canPause; }
            set
            {
                // only set this if we're not paused so the operator cannot get stuck in the menu
                if (isPaused)
                {
                    return;
                }
                _canPause = value;
            }
        }
        private bool _canPause = true;
        public bool pauseAudio = true;


        public TrackerSwitcher leftGlove;
        public TrackerSwitcher rightGlove;
        public float matchHandThreshold = 0.0f;
        // time to wait until unpausing (seconds)
        public float waitTime = 5f;
        private Coroutine matchHandsCouroutine = null;

        public SerialReader pedalDetector;
        private bool _leftPressed, _rightPressed;
        private bool oldLeft = false, oldRight = false, oldInit = true;
        private bool oldMotorEnabled = false;
        private Timer waitTimer;

        private Callbacks<bool> onPause, onUnpause;

        void Awake()
        {
            StartCoroutine(pedalDetector.readAsyncContinously(OnUpdatePresence, OnError));
            onPause = new Callbacks<bool>();
            onUnpause = new Callbacks<bool>();
        }

        private bool[] ParseData(string data)
        {
            try
            {
                if (data == null) return null;
                string[] args = data.Split(',');
                int leftInt = int.Parse(args[0]);
                int rightInt = int.Parse(args[1]);
                return new bool[] { leftInt != 0, rightInt != 0 };
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        private void OnUpdatePresence(string data)
        {
            bool[] lr = ParseData(data);
            if (lr == null)
            {
                Debug.LogError("PresenceDectector OnPresenceUpdate got invalid data");
                return;
            }

            _leftPressed = lr[0];
            _rightPressed = lr[1];
            if ((!_leftPressed || !_rightPressed) && (!oldInit || oldLeft || oldRight))
            {
                Pause();
            }
            else if ((_leftPressed && _rightPressed) && isPaused)
            {
                TryUnpause();
            }

            oldLeft = _leftPressed;
            oldRight = _rightPressed;
            oldInit = true;
        }

        private void OnError(string reason)
        {
            Debug.LogError(reason);
        }

        public bool Pause()
        {
            if (isPaused)
            {
                return false;
            }

            PauseMenu.PauseMenu.Instance.show = true;
            isPaused = true;
            Debug.Log("Paused");

            // Disable BioIK & wheelchair
            EnableControlManager.Instance.leftBioIKGroup.SetEnabled(false);
            EnableControlManager.Instance.rightBioIKGroup.SetEnabled(false);
            UnityAnimusClient.Instance._myIKHead.enabled = false;
            PedalDriver.Instance.enabled = false;

            WheelchairStateManager.Instance.SetVisibility(true, StateManager.Instance.currentState == StateManager.States.HUD ? WheelchairStateManager.HUDAlpha : 1);

            oldMotorEnabled = UnityAnimusClient.Instance.motorEnabled;
            //UnityAnimusClient.Instance.EnableMotor(false);

            if (pauseAudio)
            {
                AudioListener.pause = true;
            }

            // switch gloves to paused mode
            leftGlove.SwitchControllers();
            leftGlove.ghostHand.SetActive(true);
            rightGlove.SwitchControllers();
            rightGlove.ghostHand.SetActive(true);

            onPause.Call(true);
            return true;
        }

        public void TryUnpause()
        {
            if (!isPaused)
            {
                return;
            }
            waitTimer = new Timer();
            waitTimer.SetTimer(waitTime, timeIsUp: () =>
             {
                 StopCoroutine(matchHandsCouroutine);
                 matchHandsCouroutine = null;

                 waitTimer.ResetTimer();
                 PauseMenu.PauseMenu.Instance.matchHandsCompletion.active = false;
                 PauseMenu.PauseMenu.Instance.matchHandsCompletion.progress = 0;
                 PauseMenu.PauseMenu.Instance.matchHands.SetActive(false);
                 Unpause();
             });

            if (matchHandsCouroutine == null)
            {
                matchHandsCouroutine = StartCoroutine(MatchHands());
            }
        }

        private IEnumerator MatchHands()
        {
            while (_leftPressed && _rightPressed)
            {
                var distLeft = (leftGlove.ghostHand.transform.position
                    - EnableControlManager.Instance.leftBioIKGroup.hand_segment.gameObject.transform.position).magnitude;
                var distRight = (rightGlove.ghostHand.transform.position
                    - EnableControlManager.Instance.rightBioIKGroup.hand_segment.gameObject.transform.position).magnitude;
                //Debug.Log($"{distLeft}, {distRight}");

                PauseMenu.PauseMenu.Instance.matchHands.SetActive(true);
                if (Mathf.Max(distLeft, distRight) > matchHandThreshold)
                {
                    waitTimer.ResetTimer();
                }
                else
                {
                    waitTimer.LetTimePass(Time.deltaTime);
                    PauseMenu.PauseMenu.Instance.matchHandsCompletion.active = true;
                }
                PauseMenu.PauseMenu.Instance.matchHandsCompletion.progress = waitTimer.GetFraction();

                yield return new WaitForEndOfFrame();
            }

            //Debug.Log("stopped corountine, as one pedal is not pressed");
            matchHandsCouroutine = null;
        }


        public bool Unpause()
        {
            if (!isPaused)
            {
                return false;
            }

            Debug.Log("Unpaused");
            PauseMenu.PauseMenu.Instance.show = false;
            isPaused = false;

            // Enable BioIK & wheelchair
            EnableControlManager.Instance.leftBioIKGroup.SetEnabled(true);
            EnableControlManager.Instance.rightBioIKGroup.SetEnabled(true);
            UnityAnimusClient.Instance._myIKHead.enabled = true;

            WheelchairStateManager.Instance.SetVisibility(StateManager.Instance.currentState != StateManager.States.HUD);

            PedalDriver.Instance.enabled = true;
            //UnityAnimusClient.Instance.EnableMotor(oldMotorEnabled);

            if (pauseAudio)
            {
                AudioListener.pause = false;
            }

            // switch gloves back to control mode
            leftGlove.SwitchControllers();
            leftGlove.ghostHand.SetActive(false);
            rightGlove.SwitchControllers();
            rightGlove.ghostHand.SetActive(false);

            onUnpause.Call(false);
            return true;
        }

        public void OnPause(System.Action<bool> callback, bool once = false) => onPause.Add(callback, once);
        public void OnUnpause(System.Action<bool> callback, bool once = false) => onUnpause.Add(callback, once);
    }
}
