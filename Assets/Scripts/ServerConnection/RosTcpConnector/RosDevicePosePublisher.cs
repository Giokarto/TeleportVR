using System;
using System.Collections;
using System.Collections.Generic;
using InputDevices.VRControllers;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using PoseMsg = RosMessageTypes.Geometry.PoseMsg;
using Vector3Msg = RosMessageTypes.Geometry.Vector3Msg;

namespace ServerConnection.RosTcpConnector
{
    public class RosDevicePosePublisher : MonoBehaviour
    {

        ROSConnection ros;
        public string topicRoot = "/operator/device/";
        string topicName;
        private List<string> features = new List<string> { "pose", "velocity", "acceleration" };
        private List<string> deviceNames = new List<string> { "controller/left/", "controller/right/", "headset/" };
        public float publishMessageFrequency = 0.01f;


        private float timeElapsed;
        private PoseMsg poseMsg;
        private Vector3Msg velMsg;
        private Vector3Msg accMsg;

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

        PoseMsg GetLatestDevicePose(UnityEngine.XR.InputDevice device)
        {

            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 devicePosition);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion deviceRotation);
            var rosPosition = RosUtils.Vector2Ros(devicePosition);
            var rosOrientation = RosUtils.Quaternion2Ros(deviceRotation);

            poseMsg = new PoseMsg();

            poseMsg.position.x = rosPosition.x;
            poseMsg.position.y = rosPosition.y;
            poseMsg.position.z = rosPosition.z;

            poseMsg.orientation.x = rosOrientation.x;
            poseMsg.orientation.y = rosOrientation.y;
            poseMsg.orientation.z = rosOrientation.z;
            poseMsg.orientation.w = rosOrientation.w;

            return poseMsg;

        }

        Vector3Msg GetLatestDeviceVelocity(UnityEngine.XR.InputDevice device)
        {
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
            var rosVelocity = RosUtils.Vector2Ros(deviceVelocity);
            velMsg = new Vector3Msg();
            velMsg.x = rosVelocity.x;
            velMsg.y = rosVelocity.y;
            velMsg.z = rosVelocity.z;

            return velMsg;
        }

        Vector3Msg GetLatestDeviceAcceleration(UnityEngine.XR.InputDevice device)
        {
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceAcceleration, out Vector3 deviceAcceleration);
            var rosAccelearation = RosUtils.Vector2Ros(deviceAcceleration);
            accMsg = new Vector3Msg();
            accMsg.x = rosAccelearation.x;
            accMsg.y = rosAccelearation.y;
            accMsg.z = rosAccelearation.z;

            return accMsg;
        }

    }
}