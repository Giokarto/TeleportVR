using System;
using System.Collections.Generic;
using InputDevices;
using InputDevices.VRControllers;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace ServerConnection
{
    /// <summary>
    /// Abstract class that provides methods for communication with the server.
    /// </summary>
    public abstract class ServerBase: Singleton<ServerBase>
    {
        #region Setup, General methods and properties

        public abstract string IPaddress { get; set; }

        
        /// <summary>
        /// Hardcoded IP addresses for use either over VPN, or on local network.
        /// </summary>
        /// <param name="local"></param>
        public void UseLocalServer(bool local)
        {
            if (local)
            {
                IPaddress = "10.1.0.6";
            }
            else
            {
                IPaddress = "10.7.0.3";
            }
        }
        
        /// <summary>
        /// Is the server connection established?C:\Users\Roboy\projects\src\github.com\Roboy\TeleportVR\Assets\Scripts\ServerConnection\ServerData.cs
        /// </summary>
        public abstract bool ConnectedToServer { get; }
        
        /// <summary>
        /// Which modalities are connected
        /// </summary>
        public abstract Dictionary<Modality, bool> ModalityConnected { get; }
        
        protected void OnEnable()
        {
            VRControllerInputSystem.OnGripChange += ChangeGrip;
            InputSystem.OnLeftPrimaryButton += SendHearts;
            CreateEyeGameObjects();
        }
        private void OnDisable()
        {
            VRControllerInputSystem.OnGripChange -= ChangeGrip;
            InputSystem.OnLeftPrimaryButton -= SendHearts;
            Destroy(LeftEye);
            Destroy(RightEye);
        }

        /// <summary>
        /// To be called when switching the scene between real world and training area.
        /// (Dis)connects the operator and the robot.
        /// </summary>
        /// <param name="embody">true: enter the robot, false: exit the real world view</param>
        public void EmbodyRoboy(bool embody)
        {
            SetMotorOn(embody);
            SetPresenceIndicatorOn(embody);
            LeftEye.SetActive(embody);
            RightEye.SetActive(embody);
        }
        
        #endregion

        #region Vision
        
        /// <summary>
        /// Object to whose texture the vision textures from the server will be projected.
        /// </summary>
        public GameObject LeftEyePrefab, RightEyePrefab;
        [NonSerialized] public GameObject LeftEye, RightEye;

        /// <summary>
        /// Creates a plane / sphere from prefab to project the video to, and adds it as a child of the Eye Anchor.
        /// Why create it from the script and not in the Editor:
        ///     1. To keep the modules separate (server is one GO, but the plane has to go as a child of the Head)
        ///     2. To be able to replace the plane by a different object (Sphere) for different types of cameras
        ///     3. To not clutter up the scene when the server is not present
        /// </summary>
        public void CreateEyeGameObjects()
        {
            var anchor = GameObject.Find("LeftEyeAnchor");
            if (LeftEyePrefab == null)
            {
                Debug.Log("LeftEyePrefab not set, loading default plane");
                LeftEyePrefab = Resources.Load<GameObject>("EyePlanes/LeftEye");
            }
            LeftEye = Instantiate(LeftEyePrefab, new Vector3(0,1.068f,0) ,anchor.transform.rotation);
            LeftEye.SetLayerRecursively(LayerMask.NameToLayer("LeftEye"));
            //LeftEye.transform.parent = anchor.transform;
            
            anchor = GameObject.Find("RightEyeAnchor");
            if (RightEyePrefab == null)
            {
                Debug.Log("RightEyePrefab not set, loading default plane");
                RightEyePrefab = Resources.Load<GameObject>("EyePlanes/RightEye");
            }

            RightEye = Instantiate(RightEyePrefab, new Vector3(0, 1.068f, 0), anchor.transform.rotation);
            RightEye.SetLayerRecursively(LayerMask.NameToLayer("RightEye"));
            //RightEye.transform.parent = anchor.transform;
        }

        /// <summary>
        /// Returns the current latency in ms.
        /// </summary>
        public abstract float GetVisionLatency();
        
        /// <summary>
        /// Returns the current frames per second obtained from the server.
        /// </summary>
        /// <returns></returns>
        public abstract float GetVisionFps();
        
        /// <summary>
        /// Returns the left and right textures to which the camera image is streamed.
        /// (Those textures should stay the same during the whole session).
        /// </summary>
        public abstract Texture2D[] GetVisionTextures();
        
        #endregion

        #region Audition

        

        #endregion

        #region Voice

        

        #endregion

        #region Motor

        /// <summary>
        /// BioIK object that holds the pose of the joints.
        /// </summary>
        public BioIK.BioIK HeadIK, BodyIK;
        
        /// <summary>
        /// Enable or disable motor on the actual robot.
        /// </summary>
        /// <param name="on"></param>
        protected abstract void SetMotorOn(bool on);

        /// <summary>
        /// Callback to be called when user presses the trigger on VR controllers.
        /// </summary>
        public abstract void ChangeGrip(float left, float right);

        /// <summary>
        /// Returns the current body pose of the real robot.
        /// Can be used to save these values and restore them later (e.g. when changing the scenes).
        /// </summary>
        public abstract List<float> GetLatestJointValues();

        #endregion

        #region Emotion

        /// <summary>
        /// Indicate whether there is an operator inside of the robot.
        /// </summary>
        /// <param name="on"></param>
        protected abstract void SetPresenceIndicatorOn(bool on);

        /// <summary>
        /// Sends emotion to show on the robot.
        /// </summary>
        /// <param name="emotion">emotion to show</param>
        public abstract void SetEmotion(string emotion);

        private void SendHearts() => SetEmotion("hearts");

        #endregion
        
        #region Face Recognition

        /// <summary>
        /// Coordinates of the faces found in the camera image.
        /// </summary>
        public virtual int[][] FaceCoordinates
        {
            get { return new int[][] { }; }
        }
        
        #endregion
        
    }
}