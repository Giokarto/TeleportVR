using System.Collections.Generic;
using InputDevices;
using InputDevices.Controllers;
using UnityEngine;

namespace ServerConnection
{
    public abstract class ServerData: Singleton<ServerData>
    {
        public GameObject LeftEye;
        public GameObject RightEye;
        
        private void OnEnable()
        {
            ControllerInputSystem.OnGripChange += ChangeGrip;
            InputSystem.OnLeftPrimaryButton += SendHearts;
        }
        private void OnDisable()
        {
            ControllerInputSystem.OnGripChange -= ChangeGrip;
            InputSystem.OnLeftPrimaryButton -= SendHearts;
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
            if (embody)
            {
                SetMotorOn(true);
                SetPresenceIndicatorOn(true);
                LeftEye.SetActive(true);
                RightEye.SetActive(true);
            }
            else
            {
                SetMotorOn(false);
                SetPresenceIndicatorOn(false);
                LeftEye.SetActive(false);
                RightEye.SetActive(false);
            }
        }
    }
}