using System;
using System.Text;
using InputDevices.VRControllers;
using Newtonsoft.Json;
using RosMessageTypes.Geometry;
using ServerConnection.Aiortc;
using ServerConnection.RosTcpConnector;
using UnityEngine;
using UnityEngine.XR;
using Unity.WebRTC;

public class WebRTCHeadPositionSender : MonoBehaviour
{
    private RTCPeerConnection peerConnection;
    private RTCDataChannel dataChannel;
    private float timeElapsed;
    public float publishMessageFrequency = 0.01f;


    public void Start()
    {
        Debug.Log("dannyb initializing WebRTCHeadPositionSender");
        dataChannel = GetComponent<AiortcConnector>().dataChannel;
    }

    private void OnDataChannel(RTCDataChannel channel)
    {
        if (channel.Label == "ping")
        {
            dataChannel = channel;
        }
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

    public void Dispose()
    {
        peerConnection.OnDataChannel -= OnDataChannel;
    }
   
}