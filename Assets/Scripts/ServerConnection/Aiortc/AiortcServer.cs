using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// Implementation of <see cref="ServerData"/> with WebRTC.
    /// Uses <see cref="AiortcConnector"/> and <see cref="ImtpEncoder"/> under the hood,
    /// this is a public wrapper of these classes.
    /// </summary>
    public class AiortcServer : ServerData
    {
        [SerializeField] private AiortcConnector aiortcConnector;
        [SerializeField] private ImtpEncoder imtpEncoder;

        public void Start()
        {
            imtpEncoder.leftEye = LeftEye;
            imtpEncoder.rightEye = RightEye;
        }

        public override int[][] FaceCoordinates => aiortcConnector.faceCoordinates;

        public override bool ConnectedToServer => aiortcConnector.isConnected;

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