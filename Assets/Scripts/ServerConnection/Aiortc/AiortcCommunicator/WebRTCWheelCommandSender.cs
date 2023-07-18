using System;
using System.Collections.Generic;
using System.Linq;
using InputDevices;
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
    public class WebRTCWheelCommandSender : MonoBehaviour
    {
        private RTCPeerConnection peerConnection;
        public RTCDataChannel dataChannel;
        private float timeElapsed;
        public float publishMessageFrequency = 0.02f;
        private Dictionary<string, float> commandDict;
        
        void Start()
        {
            commandDict = new Dictionary<string, float>();
        }
        
        /// <summary>
        /// Functions copied from <see cref="ServerConnection.RosTcpConnector.RosWheelsPwmPublisher"/>
        /// </summary>
        /// <param name="leftCmd"></param>
        /// <param name="rightCmd"></param>
        void UpdateWheelCommandDict()
        {
            var linear = InputSystem.GetJoystickY();
            var angular = InputSystem.GetJoystickX();
            
            commandDict["linear"] = linear;
            commandDict["angular"] = angular;

        }

        private string GetWheelsMessage()
        {
            UpdateWheelCommandDict();
            return JsonConvert.SerializeObject(commandDict);
        }
        
        private void Update()
        {
            if (dataChannel != null && dataChannel.ReadyState == RTCDataChannelState.Open)
            {
                timeElapsed += Time.deltaTime;

                if (timeElapsed > publishMessageFrequency)
                {
                    var message = GetWheelsMessage();
                    dataChannel.Send(message);
                    //Debug.Log("Sending wheel message!\n " + message);
                    timeElapsed = 0;
                }
            }
        }
    }
}