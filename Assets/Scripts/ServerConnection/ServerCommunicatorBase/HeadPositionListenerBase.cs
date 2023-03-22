using System;
using InputDevices.VRControllers;
using RosMessageTypes.Geometry;
using ServerConnection.RosTcpConnector;
using UnityEngine;


namespace ServerConnection.ServerCommunicatorBase
{
    /*
     * Main purpose is to get incoming head position either from ros2 or webrtc and perform delay compensation
     */
    public abstract class HeadPositionListenerBase : MonoBehaviour
    {
        private HeadRotationController headRotationController;
        
        public void ProcessHeadMessage(Vector3 headVector)
        {
            (headVector.x, headVector.y) = (headVector.y, headVector.x);
            headRotationController.ProcessHeadMessage(headVector);
        }

        protected virtual void Start()
        {
            Debug.Log("DannyB start HeadPositionListenerBase");
            headRotationController = FindObjectOfType<HeadRotationController>();
        }
        
        
    }
}