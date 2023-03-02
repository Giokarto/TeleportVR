using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

namespace InputDevices.RudderPedals
{
    public class PresenceDetector : Singleton<PresenceDetector>
    {
        [System.Serializable]
        public class TrackerSwitcher
        {
            [Tooltip("CopyTransform script at SenseGlove root")]
            public RobodyControl.JointTransfer.CopyTransfrom copyTransform;
            [Tooltip("XR input for the associated hand")]
            public Transform xrController;
            [Tooltip("Ghost hand, shown when paused")]
            public GameObject ghostHand;

            private bool usingXR = false;
            public Quaternion rotationOffset = Quaternion.identity;
            public Vector3 positionOffset = Vector3.zero;

            private Transform oldController;
            private Vector3 oldPositionOffset;
            private Quaternion oldRotationOffset;

            internal void SwitchControllers()
            {
                if (usingXR)
                {
                    copyTransform.controller = oldController;
                    copyTransform.positionOffset = oldPositionOffset;
                    copyTransform.rotationOffset = oldRotationOffset;
                    usingXR = false;
                }
                else
                {
                    oldController = copyTransform.controller;
                    oldPositionOffset = copyTransform.positionOffset;
                    oldRotationOffset = copyTransform.rotationOffset;

                    // To determine this.rotationOffset and this.positionOffset  for tracked controllers, uncomment the following two lines.
                    //var posDiff = copyTransform.gameObject.transform.position - xrController.position;
                    //var rotDiff = Quaternion.Inverse(xrController.rotation) * copyTransform.gameObject.transform.rotation;
                    //Debug.Log($"{xrController.gameObject.name}: posDiff: {posDiff}, rotDiff: {rotDiff.eulerAngles}");

                    copyTransform.rotationOffset = rotationOffset;
                    copyTransform.positionOffset = positionOffset;
                    copyTransform.controller = xrController;
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
        public float holdTime = 5f;
        private Coroutine matchHandsCouroutine = null;

        public SerialReader pedalDetector;
        private bool _leftPressed, _rightPressed;
        private bool oldLeft = false, oldRight = false, oldInit = true;
        private bool oldMotorEnabled = false;
        private Timer waitTimer;

        private Callbacks<bool> onPause, onUnpause;

#if RUDDER
        void Awake()
        {
            StartCoroutine(pedalDetector.readAsyncContinously(callback: ParseData,
            onError: (error) => Debug.LogError(error))
            );
            onPause = new Callbacks<bool>();
            onUnpause = new Callbacks<bool>();
        }

        void Start()
        {
            // when switching scenes make sure any MatchHands state is reset.
            // This only occurs if switching via spacebar while MatchHands the process is running
            stateManager.onStateChangeTo[StateManager.States.HUD].Add((s) => ResetMatchHands());
            stateManager.onStateChangeTo[StateManager.States.Training].Add((s) => ResetMatchHands());
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (!isPaused)
                {
                    _leftPressed = false;
                    _rightPressed = false;
                    Pause();
                }
                else
                {
                    _leftPressed = true;
                    _rightPressed = true;
                    TryUnpause();
                }
            }
        }

        private void ParseData(string data)
        {
            try
            {
                string[] args = data.Split(',');
                int leftInt = int.Parse(args[0]);
                int rightInt = int.Parse(args[1]);
                UpdatePresence(leftInt != 0, rightInt != 0);
            }
            catch (System.Exception)
            {
                Debug.LogError($"PresenceDectector could not parse serial data: {data}");
            }
        }

        private void UpdatePresence(bool left, bool right)
        {
            _leftPressed = left;
            _rightPressed = right;
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

        public bool Pause()
        {
            if (isPaused || !canPause)
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
            Debug.Log("TryUnpause");

            waitTimer = new Timer();
            waitTimer.SetTimer(holdTime, timeIsUp: () =>
             {
                 ResetMatchHands();
                 Unpause();
             });

            if (matchHandsCouroutine == null)
            {
                matchHandsCouroutine = StartCoroutine(MatchHands());
            }
            else
            {
                Debug.Log("Match Hands Coroutine already running");
            }
        }

        private void ResetMatchHands()
        {
            if (matchHandsCouroutine != null)
            {
                StopCoroutine(matchHandsCouroutine);
                matchHandsCouroutine = null;
            }

            waitTimer.ResetTimer();
            PauseMenu.PauseMenu.Instance.matchHandsCompletion.active = false;
            PauseMenu.PauseMenu.Instance.matchHandsCompletion.progress = 0;
            PauseMenu.PauseMenu.Instance.matchHands.SetActive(false);
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
                    PauseMenu.PauseMenu.Instance.matchHandsCompletion.active = false;
                }
                else
                {
                    waitTimer.LetTimePass(Time.deltaTime);
                    PauseMenu.PauseMenu.Instance.matchHandsCompletion.active = true;
                }
                PauseMenu.PauseMenu.Instance.matchHandsCompletion.progress = waitTimer.GetFraction();

                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Stopped hands match corountine, as at least one pedal is not pressed");
            matchHandsCouroutine = null;
            ResetMatchHands();
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


        private void OnDestroy()
        {
            ResetMatchHands();
        }
#endif
    }
}
