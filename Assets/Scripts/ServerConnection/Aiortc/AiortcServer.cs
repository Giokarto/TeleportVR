using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// Implementation of <see cref="ServerBase"/> with WebRTC.
    /// Uses <see cref="AiortcConnector"/> and <see cref="ImtpEncoder"/> under the hood,
    /// this is a public wrapper of these classes.
    /// </summary>
    public class AiortcServer : ServerBase
    {
        [SerializeField] private AiortcConnector aiortcConnector;
        [SerializeField] private ImtpEncoder imtpEncoder;
        public void Start()
        {
            imtpEncoder.leftEye = LeftEye;
            imtpEncoder.rightEye = RightEye;
        }

        public override string IPaddress
        {
            get => aiortcConnector.aiortcServerURL;
            set => aiortcConnector.aiortcServerURL = value + ":8080";
        }

        public override int[][] FaceCoordinates => imtpEncoder.faceCoordinates;

        public override bool ConnectedToServer => aiortcConnector.RobotConnected;

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