using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Animus.Data;
using Animus.RobotProto;
using AnimusCommon;
using AnimusManager;
using Google.Protobuf.Collections;
using BioIK;
using ServerConnection;
#if ANIMUS_USE_OPENCV
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.Calib3dModule;
#endif
using UnityEngine;
using UnityEngine.Networking;
using Quaternion = UnityEngine.Quaternion;
using Transform = UnityEngine.Transform;
using Vector3 = UnityEngine.Vector3;

namespace AnimusClient
{
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            //Simply return true no matter what
            return true;
        }
    }

    /// <summary>
    /// <remarks>
    /// This class needs to stay in the Example folder of the AnimusClient package.
    ///
    /// The original Example scripts from the AnimusClient package are:
    /// 1. Not in a namespace (therefore global).
    /// 2. Used by <see cref="AnimusManager.AnimusClientManager"/> ('private UnityAnimusClient _client;').
    ///
    /// Creating our own scripts outside of the package would mean they have to:
    /// 1. Be named differently to avoid ambiguity with these Example scripts.
    /// 2. Inherit from the Example scripts to be usable with AnimusClientManager.
    /// 3. Not be a Singleton, otherwise the inheritance wouldn't work properly.
    ///
    /// Therefore, we keep the scripts here and make sure they don't get replaced when importing the AnimusClient package.
    /// </remarks>
    /// 
    /// This class contains methods for every modality (Vision, Voice, Motor...).
    /// 
    /// The methods for initialization, closing, setting input and processing output have to be
    /// named with a correct suffix to use in <see cref="AnimusClientManager"/>.
    ///
    /// The logic for every modality, as well as variables connected to it, are in its
    /// corresponding region to maintain cleaner code.
    /// </summary>
    public class UnityAnimusClient : ServerData
    {
        public GameObject OVRRig;
        public Transform TrackingSpace;
        public Robot chosenDetails;

        private bool bodyTransitionReady;
        private int bodyTransitionDuration = 1;
        
        public override Dictionary<Modality, bool> ModalityConnected { get; }
            = Enum.GetValues(typeof(Modality)).Cast<Enum>().ToDictionary(e => (Modality)e, v => false);

        public override bool ConnectedToServer { get; protected set; }
        
        // private List<int> imageDims;
        private RepeatedField<uint> _imageDims;
#if ANIMUS_USE_OPENCV
        private Mat yuv;
        private Mat rgb, rgb_l, yuv_left, yuv_right;
#endif

        private bool initMats;

        private const string LEDS_OFF = "off";
        private const string LEDS_CONNECTING = "robot_connecting";
        private const string LEDS_CONNECTED = "robot_established";
        private const string LEDS_IS_CONNECTED = "if_connected";

        private int rightIdx = 0, leftIdx = 0;
        private AnimusManager.AnimusClientManager animusManager;
        private bool inHUD = false;
        float emotionStamp = 0;
        Camera camRight, camLeft;

        

        private struct Undistort
        {
            public Mat mapx, mapy;
        }

        private Undistort undistort;

        public void Start()
        {
            emotion_initialise();

            motorEnabled = false;
            visionEnabled = false;
            auditionEnabled = false;
            voiceEnabled = false;
            initMats = false;
            bodyTransitionReady = false;

            camRight = RightEye.transform.GetComponentInParent<Camera>();
            camLeft = LeftEye.transform.GetComponentInParent<Camera>();
            if (!stereovision)
            {
                camRight.stereoTargetEye = StereoTargetEyeMask.None;

                camLeft.stereoTargetEye = StereoTargetEyeMask.Both;
            }
            else
            {
                camRight.stereoTargetEye = StereoTargetEyeMask.Right;

                camLeft.stereoTargetEye = StereoTargetEyeMask.Left;
            }

            latestJointValues = new List<float>();

            // controls an led ring (optional)
            StartCoroutine(SendLEDCommand(LEDS_CONNECTING));
            StartCoroutine(StartBodyTransition());

            animusManager = ClientLogic.Instance.AnimusManager;
        }

#if ANIMUS_USE_OPENCV
        private void InitUndistortion(int image_width, int image_height)
        {
            double[,] dist, camera, newCamera;
            if (image_width == 360 && image_height == 640)
            {
                dist = new double[,]
                {
                    {
                        -0.26671108837192126, 0.07901518944619403, 0.00015571985524697281, 0.0006010997461545253,
                        -0.01058019944621336
                    }
                };
                camera = new double[,]
                {
                    { 298.68813717678285, 0.0, 166.9743006819531 },
                    { 0.0, 298.0500351325468, 309.6193055827986 },
                    { 0.0, 0.0, 1.0 }
                };
                newCamera = new double[,]
                {
                    { 250.33721923828125, 0.0, 180.40019593524147 },
                    { 0.0, 240.51262156168622, 310.58069246672255 },
                    { 0.0, 0.0, 1.0 }
                };
            }
            else if (image_width == 240 && image_height == 426)
            {
                dist = new double[,]
                {
                    {
                        -0.3410095060673586, 0.13663490491793673, 0.0029517164668326173, 0.0031498764503194074,
                        -0.02582006181844203
                    }
                };
                camera = new double[,]
                {
                    { 226.1512419110776, 0.0, 109.29523311868192 },
                    { 0.0, 226.23590938020163, 205.63347777512897 },
                    { 0.0, 0.0, 1.0 }
                };
                newCamera = new double[,]
                {
                    { 183.4514923095703, 0.0, 110.754281606527 },
                    { 0.0, 165.92552947998047, 202.29188376754882 },
                    { 0.0, 0.0, 1.0 }
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            Mat distCoeffs = FillMat(dist);
            Mat cameraMatrix = FillMat(camera);
            Mat newCameraMatrix = FillMat(newCamera);

            Mat mapx = new Mat();
            Mat mapy = new Mat();
            // compute undistortion mapping & cache result
            Mat identity = Mat.eye(3, 3, CvType.CV_32FC1);
            Calib3d.initUndistortRectifyMap(cameraMatrix, distCoeffs, identity, newCameraMatrix,
                new Size(image_width, image_height), CvType.CV_32FC1, mapx, mapy);

            undistort.mapx = mapx;
            undistort.mapy = mapy;
        }

        private Mat FillMat(double[,] values)
        {
            int rows = values.GetLength(0), cols = values.GetLength(1);
            Mat mat = new Mat(rows, cols, CvType.CV_32FC1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    mat.put(i, j, values[i, j]);
                }
            }

            return mat;
        }
#endif

        IEnumerator StartBodyTransition()
        {
            yield return null;
            //robotBody.transform.eulerAngles = new Vector3(0, -180, 0);
            //
            yield return null;
            TrackingSpace = OVRRig.transform.Find("TrackingSpace");
            // humanHead = TrackingSpace.Find("CenterEyeAnchor");
            humanLeftHand = TrackingSpace.Find("LeftHandAnchor");
            humanRightHand = TrackingSpace.Find("RightHandAnchor");

            bodyTransitionReady = true;
        }

        IEnumerator SendLEDCommand(string command)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://lib.roboy.org/teleportal/" + command))
            {
                webRequest.certificateHandler = new BypassCertificate();
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();
            }
        }

        // --------------------------Vision Modality----------------------------------
        #region Vision
        // vision variables
        public bool stereovision = false;
        public bool undistortion = true;
        [SerializeField] private GameObject _leftPlane;
        [SerializeField] private GameObject _rightPlane;
        private Renderer _leftRenderer;
        private Renderer _rightRenderer;
        private Texture2D _leftTexture;
        private Texture2D _rightTexture;
        private bool visionEnabled;

        private bool triggerResChange;
        /// <summary>
        /// Setup the displays to display the received image(s) from animus.
        /// </summary>
        /// <returns>The success of this method.</returns>
        public bool vision_initialise()
        {
            //Get OVR Cameras
            var cameras = OVRRig.GetComponentsInChildren<Camera>();

            // Setup ovr camera parameters and attach component transforms to ovr camera transforms
            // This allows the planes to follow the cameras
            foreach (Camera cam in cameras)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.orthographic = true;
                cam.orthographicSize = 5;
                cam.cullingMask &= ~(1 << 11);
                switch (cam.transform.name)
                {
                    case "LeftEyeAnchor":
                        LeftEye.transform.parent = cam.transform;
                        LeftEye.transform.localPosition = Vector3.zero;
                        LeftEye.transform.localEulerAngles = Vector3.zero;
                        break;
                    case "RightEyeAnchor":
                        RightEye.transform.parent = cam.transform;
                        RightEye.transform.localPosition = Vector3.zero;
                        RightEye.transform.localEulerAngles = Vector3.zero;
                        break;
                }
            }

            _leftRenderer = _leftPlane.GetComponent<Renderer>();
            _rightRenderer = _rightPlane.GetComponent<Renderer>();
            _imageDims = new RepeatedField<uint>();

            rgb = new Mat();
            rgb_l = new Mat(rgb.rows(), rgb.cols(), CvType.CV_8UC3);


            visionEnabled = true;

            return visionEnabled;
        }

        /// <summary>
        /// Display the received image(s) from animus.
        /// </summary>
        /// <param name="currSamples">The image samples received via animus.</param>
        /// <returns>Success of this method.</returns>
        public bool vision_set(ImageSamples currSamples)
        {
            //return true;
            try
            {
                if (!bodyTransitionReady) return true;
                if (StereoPlaneMover.Instance.showingImages) return true;

                if (!visionEnabled)
                {
                    Debug.Log("Vision modality not enabled. Cannot set");
                    return false;
                }

                if (currSamples == null)
                {
                    return false;
                }

                // only set vision when it's visible
                if (StateManager.Instance.currentState != StateManager.States.HUD)
                {
                    return false;
                }

                var currSample = currSamples.Samples[0];
                var currShape = currSample.DataShape;
                //Debug.Log($"currshape: {currShape}");
                if (currShape[1] / currShape[0] >= 2)
                {
                    stereovision = true;
                }
                else
                {
                    stereovision = false;
                    //_rightPlane.SetActive(true);// stereovision && inHUD && animusManager.openModalitiesSuccess);
                }


                var all_bytes = currSample.Data.ToByteArray();
#if ANIMUS_USE_OPENCV


                if (!initMats)
                {
                    yuv = new Mat((int)(currShape[1] * 1.5), (int)(currShape[0]), CvType.CV_8UC1);

                    rgb = new Mat();
                    initMats = true;
                }

                if (all_bytes.Length != currShape[0] * currShape[1] * 1.5)
                {
                    return true;
                }

                if (currShape[0] <= 100 || currShape[1] <= 100) // TODO delete the / 5
                {
                    return true;
                }

                yuv.put(0, 0, all_bytes);

                // resize triggered
                if (_imageDims.Count == 0 || currShape[0] != _imageDims[0] || currShape[1] != _imageDims[1] ||
                    currShape[2] != _imageDims[2])
                {
                    _imageDims = currShape;
                    Debug.Log($"Resize triggered. Setting texture resolution to {currShape[0]} x {currShape[1] / 2}");
                    Debug.Log($"Setting horizontal scale to {(float)_imageDims[0]} {(float)_imageDims[1] / 2}");


                    if (stereovision)
                    {
                        if (undistortion)
                        {
                            InitUndistortion((int)_imageDims[0], (int)_imageDims[1] / 2);
                        }

                        // only half of the vertical scale corresponds to the image for one eye
                        float scaleFactor = ((float)_imageDims[1] / 2) / (float)_imageDims[0];
                        //_leftPlane.transform.localScale = new Vector3(_leftPlane.transform.localScale.x,
                        //                                              _leftPlane.transform.localScale.y,
                        //                                              scaleFactor * _leftPlane.transform.localScale.x);
                        //_rightPlane.transform.localScale = new Vector3(_rightPlane.transform.localScale.x,
                        //                                              _rightPlane.transform.localScale.y,
                        //                                              scaleFactor * _rightPlane.transform.localScale.x);

                        // the left texture is the upper half of the received image
                        _leftTexture = new Texture2D((int)_imageDims[0], (int)_imageDims[1] / 2, TextureFormat.RGB24,
                            false)
                        {
                            wrapMode = TextureWrapMode.Clamp
                        };

                        // the right texture is the lower half of the received image
                        _rightTexture = new Texture2D((int)_imageDims[0], (int)_imageDims[1] / 2, TextureFormat.RGB24,
                            false)
                        {
                            wrapMode = TextureWrapMode.Clamp
                        };
                    }
                    else
                    {
                        if (undistortion)
                        {
                            InitUndistortion((int)_imageDims[0], (int)_imageDims[1]);
                        }

                        float scaleFactor = (float)_imageDims[1] / (float)_imageDims[0];
                        //_leftPlane.transform.localScale = new Vector3(_leftPlane.transform.localScale.x,
                        //                                              _leftPlane.transform.localScale.y,
                        //                                              scaleFactor * _leftPlane.transform.localScale.x);

                        _leftTexture = new Texture2D((int)_imageDims[0], (int)_imageDims[1], TextureFormat.RGB24, false)
                        {
                            wrapMode = TextureWrapMode.Clamp
                        };
                    }
                }

                if (stereovision)
                {
                    yuv_left = yuv.rowRange(0, yuv.rows() / 2);
                    yuv_right = yuv.rowRange(yuv.rows() / 2, yuv.rows());
                    render_plane(yuv_left, _leftTexture, _leftRenderer, true);
                    render_plane(yuv_right, _rightTexture, _rightRenderer, false);
                }
                else
                {
                    render_plane(yuv, _leftTexture, _leftRenderer);
                }
#endif
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return true;
        }

        /// <summary>
        /// Closes the vision modality if it was opened and sets a corresponding flag.
        /// </summary>
        /// <returns>Closing the modality was successful.</returns>
        public bool vision_close()
        {
            if (!visionEnabled)
            {
                Debug.Log("Vision modality not enabled. Cannot close");
                return false;
            }

            visionEnabled = false;
            return true;
        }
        
        public override float GetVisionLatency()
        {
            // animusManager has private _modalitySamplers, which has private FpsLag setModalityFPS
            // Can we get it without changing even more of the code inside the Animus SDK package?
            // (using Reflection like the "good practices" of AnimusClientManager)
            throw new NotImplementedException();
            
            // TODO: find a new place for this code
            /*
            /// <summary>
            /// This method allows to display the current latency and fps in the connection widget. 
            /// </summary>
            /// <param name="latency">The current latency in ms.</param>
            /// <param name="fps">The frames per second.</param>
            public static void DisplayLatency(float latency, float fps)
            {
                // Present the latency and fps
                Widget latencyWidget = Manager.Instance.FindWidgetWithID(33);
                if (latency < 0 || latency > 100000)
                {
                    latencyWidget.GetContext().textMessage = $"FPS: {fps:F2}";
                }
                else
                {
                    latencyWidget.GetContext().textMessage = $"Latency: {latency:F2}ms\nFPS: {fps:F2}";
                }

                latencyWidget.ProcessRosMessage(latencyWidget.GetContext());

                // turn the icon from yellow (no connection) to green
                Widget wifiWidget = Manager.Instance.FindWidgetWithID(23);
                wifiWidget.GetContext().currentIcon = "WifiGreen";
                wifiWidget.ProcessRosMessage(wifiWidget.GetContext());
            }
            */
        }

        public override float GetVisionFps()
        {
            // animusManager has private _modalitySamplers, which has private FpsLag setModalityFPS
            // Can we get it without changing even more of the code inside the Animus SDK package?
            // (using Reflection like the "good practices" of AnimusClientManager)
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the textures for both eyes, so that they can be used in other scripts, e.g. for the portal.
        /// </summary>
        /// <returns>Both eye textures.</returns>
        public override Texture2D[] GetVisionTextures()
        {
            return new[] { _leftTexture, _rightTexture };
        }

        public override bool EnableVision(bool stereo)
        {
            stereovision = stereo;
            if (!animusManager.openModalitiesSuccess)
            {
                return false;
            }
            _leftPlane.SetActive(animusManager.openModalitiesSuccess);
            _rightPlane.SetActive(stereovision && animusManager.openModalitiesSuccess);
            if (!stereovision)
            {
                camRight.stereoTargetEye = StereoTargetEyeMask.None;

                camLeft.stereoTargetEye = StereoTargetEyeMask.Both;
            }
            else
            {
                camRight.stereoTargetEye = StereoTargetEyeMask.Right;

                camLeft.stereoTargetEye = StereoTargetEyeMask.Left;
            }
            return true;
        }

        public override void DisableVision()
        {
            _leftPlane.SetActive(false);
            _rightPlane.SetActive(false);
        }
        #endregion

        void render_plane(Mat yuv, Texture2D texture, Renderer renderer, bool left = true)
        {
            //Mat rgb = new Mat();
            Imgproc.cvtColor(yuv, rgb, Imgproc.COLOR_YUV2RGB_I420);
            //Debug.Log("rgb: " + rgb.size());
            //Debug.Log("yuv: " + yuv.size());
            //Imgproc.cvtColor(yuv, rgb, Imgproc.COLOR_BGR2RGB);
            //Mat rgb_l = new Mat(rgb.rows(), rgb.cols(), CvType.CV_8UC3);

            if (undistortion)
            {
                // undistort
                Imgproc.remap(rgb, rgb_l, undistort.mapx, undistort.mapy, Imgproc.INTER_LINEAR, 0);
            }
            else
            {
                rgb_l = rgb;
            }

            // display
            Utils.matToTexture2D(rgb_l, texture);
            // var name = left ? "left" : "right";
            // System.IO.File.WriteAllBytes($"{name}_{leftIdx++}.jpg", texture.EncodeToJPG());
            renderer.material.mainTexture = texture;
        }


        //--------------------------Audition Modality----------------------------------
        #region Audition
        // audition variables
        private bool auditionEnabled;
        // public GameObject Audio;
        // private AudioSetter _audioSetter;
        
        public bool audition_initialise()
        {
            return auditionEnabled;
        }

        public bool audition_set(AudioSample currSample)
        {
            if (!bodyTransitionReady) return true;
            return auditionEnabled;
        }

        public bool audition_close()
        {
            auditionEnabled = false;
            return true;
        }
        #endregion

        // --------------------------Collision Modality----------------------------------
        #region Collision
        /// <summary>
        /// Nothing to do on startup, but the method will still be called by animus.
        /// </summary>
        /// <returns>Success.</returns>
        public bool collision_initialise()
        {
            return true;
        }

        /// <summary>
        /// Receive either Link Information (first float is 1) or Collision Info (first float is 2).
        /// </summary>
        /// <param name="collision">The float array containing the Link or Collision data.</param>
        /// <returns>Success.</returns>
        public bool collision_set(Float32Array collision)
        {
#if ROSSHARP
            // subtract the first float from the length of the actual data
            int collisionLen = collision.Data.Count - 1;
            // return if the info is empty.
            if (collisionLen <= 0) return true;

            // convert the array from Float32Array to float[]
            float[] collisionArr = new float[collisionLen];
            for (int i = 0; i < collisionLen; i++)
            {
                collisionArr[i] = collision.Data[i + 1];
            }

            // if first float is 1 it's a collison
            if (collision.Data[0] > 0.5f && collision.Data[0] < 1.5 && CageInterface.cageIsConnected)
            {
                CageInterface.Instance.ForwardCollisions(collisionArr);
            }
            // if first float is a 2 it's link information
            else if (collision.Data[0] > 1.5f)
            {
                InitExoforcePublisher.StoreLinkInformation(collisionArr);
            }
#else
            Debug.LogWarning("Collision Modality active while RosSharp is deactivated.");
#endif

            return true;
        }

        /// <summary>
        /// Nothing to do on close, but the method will still be called by animus.
        /// </summary>
        /// <returns>Success.</returns>
        public bool collision_close()
        {
            return true;
        }
        #endregion

        // --------------------------Proprioception Modality----------------------------------
        #region Proprioception
        /// <summary>
        /// Nothing to do on startup, but the method will still be called by animus.
        /// </summary>
        /// <returns>Success.</returns>
        public bool proprioception_initialise()
        {
            return true;
        }

        /// <summary>
        /// Receive body parts information
        /// </summary>
        /// <param name="currSample">
        /// The float array containing the body parts statuses. The parameters are -1.0 for no connection,
        /// 1.0 for connected but a problem with the system/ motor, 0.0 connected and no problem.
        /// </param>
        /// <returns>Success.</returns>
        public bool proprioception_set(Float32Array currSample)
        {
            //print("Proprio: " + currSample.Data);
            // check if the float array contains information for the 6 body parts
            if (currSample.CalculateSize() >= 6)
            {
                /* body_manager() handles the information in currSample for the 6 body parts
                mapping: 
                41 id of the head icon, position in currSample 0
                42 id of the right_body icon, position in currSample 1
                43 id of the left_body icon, position in currSample 2
                44 id of the right_hand icon, position in currSample 3
                45 id of the left_hand icon, position in currSample 4
                46 id of the wheelchair icon, position in currSample 5
                */
                body_manager(41, 0, currSample);
                body_manager(42, 1, currSample);
                body_manager(43, 2, currSample);
                body_manager(44, 3, currSample);
                body_manager(45, 4, currSample);
                body_manager(46, 5, currSample);
            }

            return true;
        }

        /// <summary>
        /// Nothing to do on close, but the method will still be called by animus.
        /// </summary>
        /// <returns>Success.</returns>
        public bool proprioception_close()
        {
            return true;
        }
        
        /// <summary>
        /// Method to handle the information in currSample and control the color of the 6 body parts
        /// </summary>
        /// <param name="id">ID of the icon in the json file</param>
        /// <param name="position">Position of the float in currSample</param>
        /// <param name="currSample">Float array with 6 floats (-1.0; 0.0; 1.0)</param>
        public void body_manager(int id, int position, Float32Array currSample)
        {
            /* TODO: how to pass the data
            // get the instance of the widget with this id
            Widget widget = Manager.Instance.FindWidgetWithID(id);
            if ((int)(currSample.Data[position]) == -1)
            {
                // float equal to -1 then the widget changes to the icon/color at position 1 in the json file (yellow)
                widget.GetContext().currentIcon = widget.GetContext().icons[1];
            }
            else if ((int)(currSample.Data[position]) == 0)
            {
                // float equal to 0 then the widget changes to the icon/color at position 0 in the json file (green)
                widget.GetContext().currentIcon = widget.GetContext().icons[0];
            }
            else
            {
                // else or equal to 1 the widget changes to the icon/color at position 2 in the json file (red)
                widget.GetContext().currentIcon = widget.GetContext().icons[2];
            }

            // Apply the changes to the instance of the widget with ProcessRosMessage
            widget.ProcessRosMessage(widget.GetContext());
            */
        }
        #endregion


        // --------------------------Motor Modality-------------------------------------
        #region Motor
        // motor variables
        public Transform robotHead;
        public Transform robotBase;
        public Transform robotLeftHandObjective;
        public Transform robotRightHandObjective;
        private Vector3 robotLeftHandPositionROS;
        private Vector3 robotRightHandPositionROS;
        private Vector3 robotHeadPositionROS;
        private Quaternion robotLeftHandOrientationROS;
        private Quaternion robotRightHandOrientationROS;
        private Quaternion robotHeadOrientationROS;
        public Transform humanRightHand;
        public Transform humanLeftHand;
        public Transform humanHead;
        public Vector3 bodyToBaseOffset;
        public float ForwardDeadzone;
        public float SidewaysDeadzone;
        public float RotationDeadzone;
        private float humanRightHandOpen;
        private float humanLeftHandOpen;
        private Vector2 eyesPosition;
        private bool trackingRight;
        private bool trackingLeft;
        float left_open = 0, right_open = 0;

        // public NaoAnimusDriver robotDriver;
        public BioIK.BioIK _myIKBody;
        public BioIK.BioIK _myIKHead;
        private List<BioSegment> _actuatedJoints;
        private List<float> latestJointValues;
        public bool motorEnabled;
        private float _lastUpdate;

        private Animus.Data.Float32Array motorMsg;
        private Sample motorSample;

        public bool motor_initialise()
        {
            //motorEnabled = false;
            _lastUpdate = 0;
            motorMsg = new Float32Array();
            motorSample = new Sample(DataMessage.Types.DataType.Float32Arr, motorMsg);

            StartCoroutine(SendLEDCommand(LEDS_CONNECTED));
            return true;
        }

        /// <summary>
        /// Send all the necessary data from the simulated roboy to the real roboy.
        /// </summary>
        /// <returns>Success.</returns>
        public Sample motor_get()
        {
            var motorAngles = new List<float>();
            //if (Time.time * 1000 - _lastUpdate > 50)
            {
                //Debug.Log($"motor enabled: {motorEnabled}");

                // if motor not enabled - keep sending the last motor message with head pose looking down 
                if (!motorEnabled)
                {
                    if (motorMsg.Data.Count > 0)
                    {
                        // motorMsg.Data:
                        // [0] -> head_axis0
                        // [1] -> head_axis1
                        // [2] -> head_axis2
                        motorMsg.Data[0] = 0.0f;
                        motorMsg.Data[1] = 0.0f;
                        motorMsg.Data[2] = 0.0f;
                    }
                    else
                    {
                        motorAngles = new List<float>(new float[29])
                        {
                            [0] = 0.0f
                        };
                        motorMsg.Data.Clear();
                        motorMsg.Data.Add(motorAngles);
                    }

                }
                else
                {
                    //Debug.LogError("Motor modality enabled");
                    latestJointValues.Clear();
                    // head joints
                    foreach (var segment in _myIKHead.Segments)
                    {

                        if (segment.Joint != null)
                        {
                            motorAngles.Add((float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad);
                            latestJointValues.Add((float)segment.Joint.X.CurrentValue);
                        }
                    }

                    // torso joints
                    foreach (var segment in _myIKBody.Segments)
                    {
                        //Debug.Log(segment.name);
                        if (segment.Joint != null)
                        {
                            //Debug.Log($"{motorAngles.Count - 1}: {segment.gameObject.name} {segment.Joint.X.CurrentValue}");
                            motorAngles.Add((float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad);
                            latestJointValues.Add((float)segment.Joint.X.CurrentValue);
                        }
                    }

                    // Distribute angle on elbow_*_axis0 to axis0 and axis1 equally
                    const int elbowRightAxis0 = 6;
                    const int elbowLeftAxis0 = 14;
                    motorAngles[elbowRightAxis0 + 1] = -motorAngles[elbowRightAxis0] / 2;
                    motorAngles[elbowRightAxis0] /= 2;
                    motorAngles[elbowLeftAxis0 + 1] = motorAngles[elbowLeftAxis0] / 2;
                    motorAngles[elbowLeftAxis0] /= 2;


                    // right, left
#if SENSEGLOVE
                    foreach (var step in InputManager.Instance.handManager.GetMotorPositions())
                    {
                        motorAngles.Add(step);
                    }
#else
                    
                    /* // set through ChangeGrip(l ,r)
                    if (InputManager.Instance.GetLeftController())
                        InputManager.Instance.controllerLeft[0]
                            .TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out left_open);

                    if (InputManager.Instance.GetRightController())
                        InputManager.Instance.controllerRight[0]
                            .TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out right_open);
                    */


                    // 4 values for right and left
                    for (int i = 0; i < 4; i++)
                    {
                        motorAngles.Add(right_open);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        motorAngles.Add(left_open);
                    }
#endif

                    //#if RUDDER

                    //// wheelchair
                    //Vector2 wheelchairDrive = RudderPedals.PedalDriver.Instance.normalizedOutput;
                    //// left
                    //motorAngles.Add(wheelchairDrive.x);
                    //// right
                    //motorAngles.Add(wheelchairDrive.y);


                    //Debug.Log(" wheelchair: " + wheelchairDrive.x + " " + wheelchairDrive.y);

                    //#else
                    //            Vector2 axis2D;
                    //            if (!WidgetInteraction.settingsAreActive && InputManager.Instance.GetLeftController() &&
                    //                InputManager.Instance.controllerLeft[0]
                    //                    .TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out axis2D))
                    //            {
                    //                motorAngles.Add(axis2D[0]);
                    //                motorAngles.Add(axis2D[1]);
                    //            }
                    //#endif

                    motorMsg.Data.Clear();
                    motorMsg.Data.Add(motorAngles);


                }

                motorSample.Data = motorMsg;
                _lastUpdate = Time.time * 1000;

                return motorSample;
            }

            return null;
        }

        public bool motor_close()
        {
            motorEnabled = false;
            StartCoroutine(SendLEDCommand(LEDS_OFF));

            return true;
        }

        public override void ChangeGrip(float left, float right)
        {
            left_open = left;
            right_open = right;
        }
        
        protected override void SetMotorOn(bool enable)
        {
            motorEnabled = enable;
        }

        /// <summary>
        /// Can be used to debug the data send by motor_get.
        /// </summary>
        /// <param name="floatArr">The array that should be printed to the console.</param>
        private static void PrintFloatArray(float[] floatArr)
        {
            string printmsg = "";
            foreach (float f in floatArr)
            {
                printmsg += f + ", ";
            }

            print(printmsg);
        }

        #endregion

        public override List<float> GetLatestJointValues()
        {
            return latestJointValues;
        }

        private void Update()
        {
            ConnectedToServer = animusManager.connectedToRobotSuccess;
            ModalityConnected[Modality.AUDITION] = animusManager.openModalitiesSuccess;
            ModalityConnected[Modality.VOICE] = animusManager.openModalitiesSuccess;
            ModalityConnected[Modality.MOTOR] = animusManager.openModalitiesSuccess && inHUD;
        }

        // --------------------------Voice Modality----------------------------------
        #region Voice
        // voice variables
        // public GameObject Voice;
        private bool voiceEnabled;
        // private VoiceSampler _voiceSampler;
        
        public bool voice_initialise()
        {
            return voiceEnabled;
        }

        public AudioSample voice_get()
        {
            if (!bodyTransitionReady) return null;
            return null;
        }

        public bool voice_close()
        {
            voiceEnabled = false;
            return true;
        }
        #endregion

        // --------------------------Emotion Modality----------------------------------
        #region Emotion
        // emotion variables
        public bool LeftButton1;
        public bool LeftButton2;
        public bool RightButton1;
        public bool RightButton2;
        public string currentEmotion;
        public string oldEmotion;
        private Animus.Data.StringSample emotionMsg;
        private Sample emotionSample;
        public bool emotion_initialise()
        {
            emotionMsg = new StringSample();
            emotionSample = new Sample(DataMessage.Types.DataType.String, emotionMsg);
            return true;
        }

        // read out the currently pressed button combination and send it as a string via animus
        public Sample emotion_get()
        {
            if (currentEmotion.Equals(oldEmotion))
            {
                currentEmotion = "neutral";
            }
            emotionMsg.Data = currentEmotion;
            if (currentEmotion != "neutral") Debug.Log(currentEmotion);

            // send the emotion via animus to display it on the real robot
            emotionSample.Data = emotionMsg;
            oldEmotion = currentEmotion;
            return emotionSample;
        }

        /// <summary>
        /// </summary>
        /// <param name="emotion">Possible emotions: off, neutral, shy, blink, hypno_color, hearts, ?</param>
        public override void SetEmotion(string emotion)
        {
            if (Time.time - emotionStamp > 2)
            {
                currentEmotion = emotion;
                emotionStamp = Time.time;
            }
        }

        /// <summary>
        /// Necessary for animus.
        /// </summary>
        /// <returns></returns>
        public bool emotion_close()
        {
            return true;
        }

        protected override void SetPresenceIndicatorOn(bool on)
        {
            currentEmotion = on ? "tp_on" : "tp_off";
        }
        #endregion

        // Utilities

        public static Vector3 Vector2Ros(Vector3 vector3)
        {
            return new Vector3(vector3.z, -vector3.x, vector3.y);
        }

        public static Quaternion Quaternion2Ros(Quaternion quaternion)
        {
            return new Quaternion(-quaternion.z, quaternion.x, -quaternion.y, quaternion.w);
        }

        public double ClipAngle(double angle)
        {
            if (angle > 180)
            {
                angle -= 360;
            }
            else if (angle < -180)
            {
                angle += 360;
            }

            return angle;
        }
    }
}