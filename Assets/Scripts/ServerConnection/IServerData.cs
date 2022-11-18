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

        public abstract bool EnableVision(bool stereo);
        public abstract void DisableVision();
        public abstract float GetVisionLatency();
        public abstract float GetVisionFps();
        public abstract Texture2D[] GetVisionTextures();

        /// <summary>
        /// Indicate whether there is an operator inside of the robot.
        /// </summary>
        /// <param name="on"></param>
        public abstract void SetPresenceIndicatorOn(bool on);

        /// <summary>
        /// Enable or disable motor on the actual robot.
        /// </summary>
        /// <param name="on"></param>
        public abstract void SetMotorOn(bool on);

        public abstract List<float> GetLatestJointValues();
    }
}