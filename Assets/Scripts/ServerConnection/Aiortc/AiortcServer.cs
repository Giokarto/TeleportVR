using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerConnection.Aiortc
{
    public class AiortcServer : ServerData
    {
        private int fps = 10;
        
        
        public void Start()
        {
            ConnectedToServer = true;
        }
        
        public void Update()
        {
        }

        public override bool ConnectedToServer { get; protected set; }

        public override Dictionary<Modality, bool> ModalityConnected { get; }
            = Enum.GetValues(typeof(Modality)).Cast<Enum>().ToDictionary(e => (Modality)e, v => false);

        public override bool EnableVision(bool stereo)
        {
            return true;
        }

        public override void DisableVision() {}

        public override float GetVisionLatency()
        {
            return 10;
        }

        public override float GetVisionFps()
        {
            return fps;
        }

        public override Texture2D[] GetVisionTextures()
        {
            return new Texture2D[] { };
        }

        protected override void SetPresenceIndicatorOn(bool on) {}
        public override void SetEmotion(string emotion) {}
        protected override void SetMotorOn(bool on) {}
        public override void ChangeGrip(float left, float right) {}
        public override List<float> GetLatestJointValues()
        {
            return new List<float>();
        }
    }
}