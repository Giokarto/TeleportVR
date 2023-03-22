using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// Implementation of <see cref="ServerBase"/> with WebRTC.
    /// Uses <see cref="Aiortc.AiortcConnector"/> and <see cref="ImtpEncoder"/> under the hood,
    /// this is a public wrapper of these classes.
    /// </summary>
    public class AiortcServer : ServerBase
    {
        [FormerlySerializedAs("aiortcConnector")] [SerializeField] private AiortcConnector AiortcConnector;
        //[SerializeField] private ImtpEncoder imtpEncoder;
        public void Start()
        {
            AiortcConnector.LeftEye = LeftEye;
            AiortcConnector.RightEye = RightEye;
        }

        public override string IPaddress
        {
            get => AiortcConnector.aiortcServerURL;
            set => AiortcConnector.aiortcServerURL = value + ":8080";
        }

        //public override int[][] FaceCoordinates => imtpEncoder.faceCoordinates;

        public override bool ConnectedToServer => AiortcConnector.RobotConnected;

        public override Dictionary<Modality, bool> ModalityConnected { get; }
            = Enum.GetValues(typeof(Modality)).Cast<Enum>().ToDictionary(e => (Modality)e, v => false);

        public override float GetVisionLatency()
        {
            throw new NotImplementedException();
        }

        public override float GetVisionFps()
        {
            throw new NotImplementedException();
        }

        public override Texture2D[] GetVisionTextures()
        {
            return new Texture2D[] { };
        }

        protected override void SetPresenceIndicatorOn(bool on)
        {
        }

        public override void SetEmotion(string emotion)
        {
        }

        protected override void SetMotorOn(bool on)
        {
            AiortcConnector.SetMotorOn(on);
        }

        public override void ChangeGrip(float left, float right)
        {
        }

        public override List<float> GetLatestJointValues()
        {
            return new List<float>();
        }
        
    }
}