using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        [Tooltip("Serial port of the arduino")]
        public string port = "COM6";
        public int baudRate = 9600;
        [Tooltip("Time step to refresh presence detector in (seconds)")]
        public float presenceRefresh = 0.1f;
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

        private SerialReader pedalDetector;
        private bool oldLeft = false, oldRight = false, oldInit = true;
        private bool oldMotorEnabled = false;
        private Timer animationTimer;

        private Callbacks<bool> onPause, onUnpause;

        // Start is called before the first frame update
        void Start()
        {
            pedalDetector = new SerialReader(port, baudRate, refresh: presenceRefresh);
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
            if (lr == null || !canPause)
            {
                return;
            }

            bool left = lr[0], right = lr[1];
            if ((!left || !right) && (!oldInit || oldLeft || oldRight))
            {
                Pause();
            }
            else if ((left && right) && isPaused)
            {
                Unpause();
            }

            oldLeft = left;
            oldRight = right;
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

            PauseMenu.Instance.show = true;
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

        public bool Unpause()
        {
            if (!isPaused)
            {
                return false;
            }

            Debug.Log("Unpaused");
            PauseMenu.Instance.show = false;
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

        public void OnPause(System.Action<bool> callback, bool once=false) => onPause.Add(callback, once);
        public void OnUnpause(System.Action<bool> callback, bool once=false) => onUnpause.Add(callback, once);
    }
}
