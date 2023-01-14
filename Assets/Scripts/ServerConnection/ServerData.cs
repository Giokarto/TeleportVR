using System;
using System.Collections.Generic;
using InputDevices;
using InputDevices.Controllers;
using UnityEditor;
using UnityEngine;

namespace ServerConnection
{
    public abstract class ServerData: Singleton<ServerData>
    {
        public GameObject LeftEyePrefab;
        public GameObject RightEyePrefab;
        [SerializeField] protected GameObject LeftEye;
        [SerializeField] protected GameObject RightEye;

        /// <summary>
        /// Creates a plane from prefab to project the video to, and adds it as a child of the Eye Anchor.
        /// Why create it from the script and not in the Editor:
        ///     1. To keep the modules separate (server is one GO, but the plane has to go as a child of the Head)
        ///     2. To be able to replace the plane by a different object (Sphere) for different types of cameras
        ///     3. To not clutter up the scene when the server is not present
        /// </summary>
        protected void CreateLeftEye()
        {
            var anchor = GameObject.Find("LeftEyeAnchor");
            if (LeftEyePrefab == null)
            {
                Debug.Log("LeftEyePrefab not set, loading default plane");
                LeftEyePrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/EyePlanes/LeftEye.prefab", typeof(GameObject)) as GameObject;
            }
            LeftEye = Instantiate(LeftEyePrefab, anchor.transform);
        }
        
        /// <summary>
        /// See <see cref="CreateLeftEye"/>.
        /// </summary>
        protected void CreateRightEye()
        {
            var anchor = GameObject.Find("RightEyeAnchor");
            if (RightEyePrefab == null)
            {
                Debug.Log("RightEyePrefab not set, loading default plane");
                RightEyePrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/EyePlanes/RightEye.prefab", typeof(GameObject)) as GameObject;
            }
            RightEye = Instantiate(RightEyePrefab, anchor.transform);
        }
        
        private void OnEnable()
        {
            ControllerInputSystem.OnGripChange += ChangeGrip;
            InputSystem.OnLeftPrimaryButton += SendHearts;
            CreateLeftEye();
            CreateRightEye();
        }
        private void OnDisable()
        {
            ControllerInputSystem.OnGripChange -= ChangeGrip;
            InputSystem.OnLeftPrimaryButton -= SendHearts;
            Destroy(LeftEye);
            Destroy(RightEye);
        }

        public virtual int[][] FaceCoordinates
        {
            get { return new int[][] { }; }
        }

        public abstract bool ConnectedToServer { get; protected set; }
        
        public abstract Dictionary<Modality, bool> ModalityConnected { get; }

        public abstract bool EnableVision(bool stereo);

        public abstract void DisableVision();

        public abstract float GetVisionLatency();
        public abstract float GetVisionFps();
        public abstract Texture2D[] GetVisionTextures();

        /// <summary>
        /// Indicate whether there is an operator inside of the robot.
        /// </summary>
        /// <param name="on"></param>
        protected abstract void SetPresenceIndicatorOn(bool on);

        /// <summary>
        /// Sends emotion through the data channel that the robot should show.
        /// </summary>
        /// <remarks>TODO: change string to enum</remarks>
        /// <param name="emotion">emotion to show</param>
        public abstract void SetEmotion(string emotion);

        private void SendHearts() => SetEmotion("hearts");

        /// <summary>
        /// Enable or disable motor on the actual robot.
        /// </summary>
        /// <param name="on"></param>
        protected abstract void SetMotorOn(bool on);

        public abstract void ChangeGrip(float left, float right);

        public abstract List<float> GetLatestJointValues();

        /// <summary>
        /// To be called when switching the scene between real world and training area.
        /// </summary>
        /// <param name="embody">true: enter the robot, false: exit the real world view</param>
        public void EmbodyRoboy(bool embody)
        {
            SetMotorOn(embody);
            SetPresenceIndicatorOn(embody);
            LeftEye.SetActive(embody);
            RightEye.SetActive(embody);
        }
    }
}