using System;
using System.Collections.Generic;
using System.Text;
using InputDevices.VRControllers;
using Newtonsoft.Json;
using RosMessageTypes.Geometry;
using ServerConnection.Aiortc;
using ServerConnection.RosTcpConnector;
using ServerConnection.ServerCommunicatorBase;
using UnityEngine;
using UnityEngine.XR;
using Unity.WebRTC;

namespace ServerConnection.Aiortc
{
    public class WebRTCJointPositionSender : DevicePoseSenderBase
    {
        private RTCPeerConnection peerConnection;
        private RTCDataChannel dataChannel;
        private float timeElapsed;
        public float publishMessageFrequency = 0.01f;
        
        private BioIK.BioIK HeadIK;
        private BioIK.BioIK BodyIK;


        public void Start()
        {
            Debug.Log("dannyb initializing WebRTCJointPositionSender");
            dataChannel = GetComponent<AiortcConnector>().jsDataChannel;
            
            BodyIK = ServerBase.Instance.BodyIK;
            HeadIK = ServerBase.Instance.HeadIK;
        }

        private void SendMessage()
        {
            var json = JsonConvert.SerializeObject(GetLatestHeadPose());
            dataChannel.Send(json);
        }

        public WebRTCMessages.HeadPositionMessage GetLatestHeadPose()
        {
            var message = new WebRTCMessages.HeadPositionMessage()
            {
                head_axis0 = 0f,
                head_axis1 = 0f,
                head_axis2 = 0f
            };
            foreach (var segment in HeadIK.Segments)
            {
                if (segment.Joint != null)
                {
                    if (segment.Joint.name == "head_axis2")
                        message.head_axis2 = (float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad;
                    if (segment.Joint.name == "head_axis1")
                        message.head_axis1 = (float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad;
                    if (segment.Joint.name == "head_axis0")
                        message.head_axis0 = (float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad;
                }
            }

            return message;
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;

            if (timeElapsed > publishMessageFrequency)
            {
                SendMessage();
                timeElapsed = 0;
            }
        }
    }
}