using System;
using System.Collections;
using System.Collections.Generic;
using InputDevices.VRControllers;
using ServerConnection.ServerCommunicatorBase;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using PoseMsg = RosMessageTypes.Geometry.PoseMsg;
using Vector3Msg = RosMessageTypes.Geometry.Vector3Msg;

namespace ServerConnection.RosTcpConnector
{
    public class RosDevicePosePublisher : DevicePoseSenderBase
    {

        ROSConnection ros;
        public string topicRoot = "/operator/device/";
        string topicName;
        private List<string> features = new List<string> { "pose", "velocity", "acceleration" };
        private List<string> deviceNames = new List<string> { "controller/left/", "controller/right/", "headset/" };
        public float publishMessageFrequency = 0.01f;


        private float timeElapsed;

        // Start is called before the first frame update
        void Start()
        {
            ros = ROSConnection.GetOrCreateInstance();

            foreach (var device in deviceNames)
            {

                topicName = topicRoot + device;
                ros.RegisterPublisher<PoseMsg>(topicName + "pose");
                ros.RegisterPublisher<Vector3Msg>(topicName + "velocity");
                ros.RegisterPublisher<Vector3Msg>(topicName + "acceleration");

            }
        }

        // Update is called once per frame
        void Update()
        {
            timeElapsed += Time.deltaTime;

            if (timeElapsed > publishMessageFrequency)
            {
                foreach (var device in deviceNames)
                {
                    topicName = topicRoot + device;
                    var inputDevice = VRControllerInputSystem.GetDeviceByName(device);
                    if (inputDevice != null)
                    {
                        if (inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out var _))
                        {
                            if (device.Contains("head"))
                            {
                                var qm = GetLatestDevicePose(inputDevice).orientation;
                                Quaternion q = new Quaternion((float)qm.y, (float)-qm.z, (float)qm.x, (float)qm.y);
                                Debug.Log($"device pose is {q.x}, {q.y}, {q.z}");
                            }
                            ros.Publish(topicName + "pose", GetLatestDevicePose(inputDevice));
                            ros.Publish(topicName + "velocity", GetLatestDeviceVelocity(inputDevice));
                            ros.Publish(topicName + "acceleration", GetLatestDeviceAcceleration(inputDevice));
                        }
                        else
                        {
                            Debug.LogWarning($"{inputDevice.name} is not tracked");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"{device} is not available");
                    }

                }

                timeElapsed = 0;
            }
        }

    }
}