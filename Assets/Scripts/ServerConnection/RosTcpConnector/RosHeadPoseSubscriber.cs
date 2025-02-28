using System;
using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using ServerConnection.ServerCommunicatorBase;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using CompressedImage = RosMessageTypes.Sensor.CompressedImageMsg;
using Image = RosMessageTypes.Sensor.ImageMsg;

namespace ServerConnection.RosTcpConnector
{
    public class RosHeadPoseSubscriber : HeadPositionListenerBase
    {
        //public ROSConnection ros;
        public MeshRenderer meshRenderer;
        public string topic;


        // Start is called before the first frame update
        void Start()
        {
            base.Start();
            Debug.Log("initializing RosHeadPositionSubscriber");
            ROSConnection.GetOrCreateInstance().Subscribe<JointStateMsg>("/roboy/pinky/simulation/joint_targets", ProcessJointMessage);
        }

        void ProcessJointMessage(JointStateMsg jointState)
        {
            //handle message
            string headAxis0 = "head_axis0";
            string headAxis1 = "head_axis1";
            string headAxis2 = "head_axis2";
            
            int index_axis0 = Array.IndexOf(jointState.name, headAxis0);
            int index_axis1 = Array.IndexOf(jointState.name, headAxis1);
            int index_axis2 = Array.IndexOf(jointState.name, headAxis2);
            
            if (index_axis0 >= 0 && index_axis1 >= 0 && index_axis2 >= 0)
            {
                Vector3 headVector = new Vector3((float)jointState.position[index_axis0],
                    (float)jointState.position[index_axis1],
                    (float)jointState.position[index_axis2]);
                ProcessHeadMessage(headVector);
            }
        }

    }
}