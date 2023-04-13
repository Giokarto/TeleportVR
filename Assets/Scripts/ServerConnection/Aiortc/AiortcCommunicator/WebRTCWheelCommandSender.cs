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
        public int PWM_MIN = 0;
        public int PWM_MAX = 30;
        public string topicRoot = "/operator/wheels/";
        private string leftTopicName, rightTopicName;
        private Dictionary<string, short> commandDict;
        
        void Start()
        {
            leftTopicName = topicRoot + "left";
            rightTopicName = topicRoot + "right";
            
            commandDict = new Dictionary<string, short>()
            {
                {leftTopicName, 0},
                {rightTopicName, 0}
            };
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

            double l = linear - angular / 2;
            double r = linear + angular / 2;

            double[] drive = new double[] { l, r };
            drive = drive.Select(x => Math.Min(1, Math.Max(-1, x))).ToArray();

            drive = MinPower(drive, min_angular: 20 / PWM_MAX, max_angular: 31 / PWM_MAX, min_linear: 20 / PWM_MAX);
            double[] pwm = drive.Select(x => Math.Sign(x) * MapPwm(Math.Abs(x), PWM_MIN, PWM_MAX)).ToArray();
            
            commandDict[leftTopicName] = (short)Math.Round(pwm[0]);
            commandDict[rightTopicName] = (short)Math.Round(pwm[1]);

        }

        double Map(double x, double start = 0.1, double min_power = 0.1, double max_power = 1)
        {
            double m = (max_power - min_power) / (1 - start);
            if (Math.Abs(x) <= start)
            {
                // cubic hermite interpolation in abs(x) <= start
                double h_01(double t) => -2 * Math.Pow(t, 3) + 3 * Math.Pow(t, 2);
                double h_11(double t) => Math.Pow(t, 3) - Math.Pow(t, 2);
                return Math.Sign(x) * h_01(Math.Abs(x) / start) * min_power + h_11(Math.Abs(x) / start) * start * m;
            }
            else
            {
                // linear interpolation in start < abs(x)
                double linear(double a) => m * (a - start) + min_power;
                return Math.Sign(x) * linear(Math.Abs(x));
            }
        }

        double[] MinPower(double[] x, double min_angular = 0.1, double max_angular = 1, double min_linear = 0.1,
            double max_linear = 1)
        {
            // rotation & scaling matrices to convert L/R to angular/linear representation
            double[,] R = new double[,] { { 0.5, -0.5 }, { 0.5, 0.5 } };
            double[,] R_inv = new double[,] { { 1, 1 }, { -1, 1 } };
            double ang = R[0, 0] * x[0] + R[0, 1] * x[1];
            double lin = R[1, 0] * x[0] + R[1, 1] * x[1];
            double[] mapped = new double[]
            {
                Map(ang, start: 0.01, min_power: min_angular, max_power: max_angular),
                Map(lin, start: 0.05, min_power: min_linear, max_power: max_linear)
            };
            ang = R_inv[0, 0] * mapped[0] + R_inv[0, 1] * mapped[1];
            lin = R_inv[1, 0] * mapped[0] + R_inv[1, 1] * mapped[1];
            return new double[] { ang, lin };
        }

        double MapPwm(double x, double out_min, double out_max)
        {
            return x * (out_max - out_min) + out_min;
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