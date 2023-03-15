using System;
using System.Collections.Generic;
using System.Text;
using InputDevices.VRControllers;
using Newtonsoft.Json;
using RosMessageTypes.Geometry;
using ServerConnection.Aiortc;
using ServerConnection.RosTcpConnector;
using UnityEngine;
using UnityEngine.XR;
using Unity.WebRTC;

public class WebRTCJointPositionSender : MonoBehaviour
{
    private List<string> deviceNames = new List<string> { "controller/left/", "controller/right/", "headset/" };
    private RTCPeerConnection peerConnection;
    private RTCDataChannel dataChannel;
    private float timeElapsed;
    public float publishMessageFrequency = 0.01f;


    public void Start()
    {
        Debug.Log("dannyb initializing WebRTCHeadPositionSender");
        dataChannel = GetComponent<AiortcConnector>().jsDataChannel;
    }

    private void SendMessage()
    {
        var json = JsonConvert.SerializeObject(GetLatestHeadPose());
        dataChannel.Send(json);
    }
    
    public WebRTCHeadPositionListener.HeadPositionMessage GetLatestHeadPose()
    {
        var device = VRControllerInputSystem.GetDeviceByName("headset/");
        device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 devicePosition);
        var rosPosition = RosUtils.Vector2Ros(devicePosition);
        string[] name;
        double[] position;
        double[] velocity;
        /*
        foreach (var device in deviceNames)
        {
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

        }*/
        return new WebRTCHeadPositionListener.HeadPositionMessage
        {
            head_axis0 = rosPosition.x,
            head_axis1 = rosPosition.y,
            head_axis2 = rosPosition.z
        };
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