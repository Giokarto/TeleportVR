using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerConnection
{
    public interface IServerData
    {
        public bool ConnectedToServer { get; }
        
        public Dictionary<Modality, bool> ModalityConnected { get; }

        public bool EnableVision(bool stereo);
        public void DisableVision();
        public float GetVisionLatency();
        public float GetVisionFps();
        public Texture2D[] GetVisionTextures();

        /// <summary>
        /// Indicate whether there is an operator inside of the robot.
        /// </summary>
        /// <param name="on"></param>
        public void SetPresenceIndicatorOn(bool on);

        /// <summary>
        /// Sends emotion through the data channel that the robot should show.
        /// </summary>
        /// <remarks>TODO: change string to enum</remarks>
        /// <param name="emotion">emotion to show</param>
        public void SetEmotion(string emotion);

        /// <summary>
        /// Enable or disable motor on the actual robot.
        /// </summary>
        /// <param name="on"></param>
        public void SetMotorOn(bool on);

        public void ChangeGrip(float left, float right);

        public List<float> GetLatestJointValues();
    }
}