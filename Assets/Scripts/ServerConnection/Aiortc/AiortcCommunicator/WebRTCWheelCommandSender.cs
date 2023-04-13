using System;
using Newtonsoft.Json;
using ServerConnection.RosTcpConnector;
using ServerConnection.ServerCommunicatorBase;
using UnityEngine;
using Unity.WebRTC;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// This class handles of initialization and sends operator's head position to joint states data channel
    /// </summary>
    public class WebRTCWheelCommandSender
    {
        private RTCPeerConnection peerConnection;
        public RTCDataChannel dataChannel;
        private float timeElapsed;
        public float publishMessageFrequency = 0.02f;
        public int PWM_MIN = 0;
        public int PWM_MAX = 30;


        public void Start()
        {
            Debug.Log("initializing WebRTCJointPositionSender");
        }

        /// <summary>
        /// Gatheres joint states and converts them to a string
        /// </summary>
        /// <returns>
        /// json string of joint states
        /// </returns>
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
            if (dataChannel != null && dataChannel.ReadyState == RTCDataChannelState.Open)
            {
                timeElapsed += Time.deltaTime;

                if (timeElapsed > publishMessageFrequency)
                {
                    var message = GetJointPositionsMessage();
                    dataChannel.Send(message);
                    // Debug.Log("Sending joint poses!\n " + message);
                    timeElapsed = 0;
                }
            }
        }
    }
}