using System.Collections;
using System.Collections.Generic;
using ServerConnection.RosTcpConnector;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.WebRTC;

public enum HeadPositionProtocol
{
    ROS2,
    WebRTC
}

public class HeadPositionListenerFactory : MonoBehaviour
{
    /*public HeadPositionProtocol protocol = HeadPositionProtocol.ROS2;

    // ROS2-specific fields
    public string ros2Topic = "/head/position";

    // WebRTC-specific fields
    public string dataChannelName = "head_position";
    public WebRTCConnectionManager connectionManager;

    public HeadPositionListenerBase CreateListener()
    {
        switch (protocol)
        {
            case HeadPositionProtocol.ROS2:
                return new RosHeadPositionSubscriber();
            case HeadPositionProtocol.WebRTC:
                RTCDataChannel dc = connectionManager.GetDataChannel(dataChannelName);
                return new WebRTCHeadPositionListener(dc);
            default:
                Debug.LogError("Unsupported head position protocol: " + protocol.ToString());
                return null;
        }
    }*/
}
