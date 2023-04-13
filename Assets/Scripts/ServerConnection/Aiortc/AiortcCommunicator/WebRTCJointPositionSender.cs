using System;
using System.Collections.Generic;
using InputDevices.VRControllers;
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
    public class WebRTCJointPositionSender : DevicePoseSenderBase
    {
        private RTCPeerConnection peerConnection;
        public RTCDataChannel dataChannel;
        private float timeElapsed;
        public float publishMessageFrequency = 0.01f;

        private List<string> fingerJointNames = new List<string> { "thumb_", "index_", "middle_", "pinky_" };

        public void Start()
        {
            Debug.Log("initializing WebRTCJointPositionSender");
        }

        private float leftGrip, rightGrip;

        private void OnEnable()
        {
            // Change grip through the input system, don't send it directly from reading the controllers.
            // This way changing the grip state can be paused, e.g. when a menu is open.
            VRControllerInputSystem.OnGripChange += SaveGripState;
        }

        private void OnDisable()
        {
            VRControllerInputSystem.OnGripChange -= SaveGripState;
        }
        public void SaveGripState(float left, float right)
        {
            leftGrip = left;
            rightGrip = right;
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
            
            // TODO sort out magical numbers as in ROS publisher, distribute values equally in elbows

            // hand - fingers
            // copied from RosJointPosePublisher
            for (int i = 0; i < 4; i++)
            {
                jointPositions.Add(fingerJointNames[i] + "right", rightGrip);
            }

            for (int i = 0; i < 4; i++)
            {
                jointPositions.Add(fingerJointNames[i] + "left", leftGrip);
            }
            
            return JsonConvert.SerializeObject(jointPositions);
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
                    //Debug.Log("Sending joint poses!\n " + message);
                    timeElapsed = 0;
                }
            }
        }
    }
}