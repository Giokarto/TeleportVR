using System.Collections;
using System.Collections.Generic;
using CurvedUI;
using ServerConnection.Aiortc;
using ServerConnection.RosTcpConnector;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.WebRTC;

namespace ServerConnection.Aiortc
{
    public enum HeadPositionProtocol
    {
        ROS2,
        WebRTC
    }

    public class HeadPositionListenerFactory : MonoBehaviour
    {
        public static void CreateListener(AiortcConnector gameObject, HeadPositionProtocol protocol)
        {
            switch (protocol)
            {
                case HeadPositionProtocol.ROS2:
                    gameObject.AddComponentIfMissing<RosHeadPoseSubscriber>();
                    break;
                case HeadPositionProtocol.WebRTC:
                    gameObject.AddComponentIfMissing<RosHeadPoseSubscriber>();
                    break;
            }
        }
    }
}