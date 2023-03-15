using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace ServerConnection.RosTcpConnector
{
    public class RosServerData : ServerData
    {
        private ROSConnection ros;
        private Light light;
        
        public RosImageSubscriber vision;
        public RosJointPosePublisher jointPose;
        public RosDevicePosePublisher headPose;
        public RosAudioDataHandler audio;
        public RosWheelsPwmPublisher wheels;
        private void Start()
        {
            ros = ROSConnection.GetOrCreateInstance();
            light = FindObjectOfType<Light>();
        }

        private new void OnEnable()
        {
            base.OnEnable();
            LeftEye.transform.position = new Vector3(0, 0, 8);
            RightEye.transform.position = new Vector3(0, 0, 8);

        }

        private void Update()
        {
            ModalityConnected[Modality.MOTOR] = jointPose.enabled && headPose.enabled;
            ModalityConnected[Modality.VOICE] = audio.isActiveAndEnabled;
            ModalityConnected[Modality.VISION] = vision.isActiveAndEnabled;
            ModalityConnected[Modality.EMOTION] = false;
            ModalityConnected[Modality.AUDITION] = audio.isActiveAndEnabled;
        }

        public override string IPaddress
        {
            get => ros.RosIPAddress;
            set
            {
                ros.RosIPAddress = value;
            }
        }

        public override bool ConnectedToServer
        {
            get => !ros.HasConnectionError;
        }

        public override Dictionary<Modality, bool> ModalityConnected { get; }
            = Enum.GetValues(typeof(Modality)).Cast<Enum>().ToDictionary(e => (Modality)e, v => false);
        
        public override float GetVisionLatency()
        {
            throw new System.NotImplementedException();
        }

        public override float GetVisionFps()
        {
            return vision.FPS;
        }

        public override Texture2D[] GetVisionTextures()
        {
            throw new System.NotImplementedException();
        }

        protected override void SetMotorOn(bool on)
        {
            jointPose.enabled = on;
            headPose.enabled = on;
            wheels.enabled = on;
            light.enabled = on;
            audio.enabled = on;
        }

        public override void ChangeGrip(float left, float right)
        {
        }
        
        public override List<float> GetLatestJointValues()
        {
            throw new NotImplementedException();
        }

        protected override void SetPresenceIndicatorOn(bool on)
        {
            // no presence indicator
        }

        public override void SetEmotion(string emotion)
        {
            // no emotions
        }
    }
}