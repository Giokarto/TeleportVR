using System;
using Newtonsoft.Json;
using ServerConnection.RosTcpConnector;
using ServerConnection.ServerCommunicatorBase;
using UnityEngine;
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

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private string GetJointPositionsMessage()
        {
            var jointPositions = RobodyControl.RobotMotionManager.Instance.GetAllCurrentJointStates();
            return JsonConvert.SerializeObject(jointPositions);
            
            // TODO sort out magical numbers as in ROS publisher, distribute values equally in elbows

            // hand - fingers
            // TODO
        }

        private void Update()
        {
            if (dataChannel == null)
            {
                dataChannel = FindObjectOfType<AiortcConnector>().jsDataChannel;
            }
            timeElapsed += Time.deltaTime;

            if (timeElapsed > publishMessageFrequency)
            {
                var message = GetJointPositionsMessage();
                dataChannel.Send(message);
                timeElapsed = 0;
            }
        }
    }
}