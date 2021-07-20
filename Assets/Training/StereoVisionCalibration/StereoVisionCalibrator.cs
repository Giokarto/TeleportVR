using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Training.Calibration
{
    public class StereoVisionCalibrator : Singleton<StereoVisionCalibrator>
    {
        public bool calibrating = false;
        public Texture2D leftCalibrationTexture, rightCalibrationTexture;
        public HandCalibrator leftCalibrator;
        [Range(0, 1)] public float moveSpeed = 0.1f;

        public GameObject leftEyePlane, rightEyePlane;
        private bool coroutineRunning = false;
        public Renderer leftRenderer, rightRenderer;
        private Texture oldLeftTexture, oldRightTexture;
        private bool oldLeftActive, oldRightActive;
        private Dictionary<KeyCode, Vector3> moveDict;
        private Vector3 diff;
        private readonly string FQN = typeof(StereoVisionCalibrator).FullName;

        private Vector3 lastControllerPos;
        private bool lastControllerPosInit = false;

        private List<System.Action<Vector3>> onDoneCallbacks = new List<System.Action<Vector3>>();
        private bool init = false;

        // Start is called before the first frame update
        void Start()
        {
            //if (leftObj != null)
            //{
            //    leftEyePlane = leftObj.gameObject;
            //}
            //else
            //{
            //    Debug.LogError($"{FQN} could not find XRLeftEyePlane GameObject");
            //}

            //if (rightObj != null)
            //{
            //    rightEyePlane = rightObj.gameObject;
            //}
            //else
            //{
            //    Debug.LogError($"{FQN} could not find XRRightEyePlane GameObject");
            //}

            //    moveDict = new Dictionary<KeyCode, Vector3>
            //{
            //    {KeyCode.W, -leftEyePlane.transform.forward},
            //    {KeyCode.A, leftEyePlane.transform.right},
            //    {KeyCode.S, leftEyePlane.transform.forward},
            //    {KeyCode.D, -leftEyePlane.transform.right},
            //};
        }

        void Update()
        {

            // wait for objects to be loaded
            if (init)
            {
                return;
            }
            if (leftEyePlane == null || rightEyePlane == null)
            {
                var leftObj = FindObjectOfType<ObjRef.XRLeftEyePlane>(true);
                var rightObj = FindObjectOfType<ObjRef.XRRightEyePlane>(true);
                if (leftObj != null && rightObj != null)
                {
                    leftEyePlane = leftObj.gameObject;
                    rightEyePlane = rightObj.gameObject;
                }
                leftRenderer = leftEyePlane.GetComponent<Renderer>();
                rightRenderer = rightEyePlane.GetComponent<Renderer>();
                Load();
                init = true;
            }
        }

        public void StartCalibration()
        {
            if (leftEyePlane == null || rightEyePlane == null)
            {
                Debug.LogError("Cannot Start calibration when not all eye planes have been found");
                return;
            }
            if (coroutineRunning)
            {
                Debug.Log("Calibration is already running");
                return;
            }
            coroutineRunning = true;
            StartCoroutine(Calibration());
        }

        public void StopCalibration()
        {
            calibrating = false;
            StopAllCoroutines();
            coroutineRunning = false;
            // potentially de-initialization is not run when stopping the coroutine.
            // Therefore, run it again to be safe.
            DeInitCalibration();
        }

        public void OnDone(System.Action<Vector3> callback)
        {
            onDoneCallbacks.Add(callback);
        }

        private void InitCalibration()
        {
            // init & save old values
            oldLeftTexture = leftRenderer.material.mainTexture;
            oldRightTexture = rightRenderer.material.mainTexture;
            oldLeftActive = leftEyePlane.activeSelf;
            oldRightActive = rightEyePlane.activeSelf;

            leftRenderer.material.mainTexture = leftCalibrationTexture;
            leftRenderer.material.SetTextureScale("_MainTex", new Vector2(0.1f, 0.1f));
            rightRenderer.material.SetTextureScale("_MainTex", new Vector2(0.1f, 0.1f));
            rightRenderer.material.mainTexture = rightCalibrationTexture;
            leftEyePlane.SetActive(true);
            rightEyePlane.SetActive(true);
        }

        IEnumerator Calibration()
        {
            Debug.Log($"Start Stereo Vision Calibration");
            InitCalibration();

            while (calibrating && !DetectDone())
            {
                var dir = GetMoveDirection(InputManager.Instance.controllerLeftObj.transform);
                leftEyePlane.transform.position += moveSpeed * dir;
                yield return new WaitForFixedUpdate();
            }

            // average positon devation
            diff = leftEyePlane.transform.position - rightEyePlane.transform.position;
            Debug.Log($"Done calibrating stereo vision, eye difference {diff.magnitude}m");
            leftEyePlane.transform.position += -diff / 2;
            rightEyePlane.transform.position += -diff / 2;

            Save();
            DeInitCalibration();

            // call onDone callbacks
            foreach (var callback in onDoneCallbacks)
            {
                callback(diff);
            }
            coroutineRunning = false;
            yield break;
        }

        private Vector3 GetMoveDirection(Transform controller)
        {
            var pos = controller.position;
            if (!lastControllerPosInit)
            {
                lastControllerPos = pos;
                lastControllerPosInit = true;
                return Vector3.zero;
            }

            var diff = pos - lastControllerPos;
            lastControllerPos = pos;

            // project diff to xy plane
            diff.z = 0;
            return diff;
        }

        private bool DetectDone()
        {
            return leftCalibrator.handFound 
                && leftCalibrator.PoseError(HandPose.ThumbUp) <= leftCalibrator.testParams.maxError;
        }

        public void DeInitCalibration()
        {
            // revert initialization
            leftEyePlane.SetActive(oldLeftActive);
            rightEyePlane.SetActive(oldRightActive);
            leftRenderer.material.mainTexture = oldLeftTexture;
            rightRenderer.material.mainTexture = oldRightTexture;
        }

        private void Save()
        {
            // saved flag
            PlayerPrefX.SetInt(FQN, 1);
            PlayerPrefX.SetVector3($"{FQN}_left_position", leftEyePlane.transform.localPosition);
            PlayerPrefX.SetVector3($"{FQN}_right_position", rightEyePlane.transform.localPosition);
            //PlayerPrefs.SetFloat($"{FQN}_left_position.x", leftEyePlane.transform.localPosition.x);
            //PlayerPrefs.SetFloat($"{FQN}_left_position.y", leftEyePlane.transform.localPosition.y);
            //PlayerPrefs.SetFloat($"{FQN}_left_position.z", leftEyePlane.transform.localPosition.z);
            //PlayerPrefs.SetFloat($"{FQN}_right_position.x", rightEyePlane.transform.localPosition.x);
            //PlayerPrefs.SetFloat($"{FQN}_right_position.y", rightEyePlane.transform.localPosition.y);
            //PlayerPrefs.SetFloat($"{FQN}_right_position.z", rightEyePlane.transform.localPosition.z);
            PlayerPrefX.Save();
            Debug.Log($"Saved {FQN} preferences");
        }

        private void Load()
        {
            if (!PlayerPrefX.HasKey(FQN))
            {
                Debug.LogWarning("Could not load StereoVision calibration: No preferences saved");
                return;
            }
            leftEyePlane.transform.localPosition = PlayerPrefX.GetVector3($"{FQN}_left_position");
            rightEyePlane.transform.localPosition = PlayerPrefX.GetVector3($"{FQN}_right_position");
            //leftEyePlane.transform.localPosition = new Vector3(
            //    PlayerPrefs.GetFloat($"{FQN}_left_position.x"),
            //    PlayerPrefs.GetFloat($"{FQN}_left_position.y"),
            //    PlayerPrefs.GetFloat($"{FQN}_left_position.z"));
            //rightEyePlane.transform.localPosition = new Vector3(
            //    PlayerPrefs.GetFloat($"{FQN}_right_position.x"),
            //    PlayerPrefs.GetFloat($"{FQN}_right_position.y"),
            //    PlayerPrefs.GetFloat($"{FQN}_right_position.z"));
            Debug.Log($"Successfully loaded {FQN} preferences");
        }
    }

}
