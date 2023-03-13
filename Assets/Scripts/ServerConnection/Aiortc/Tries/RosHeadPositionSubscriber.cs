using System;
using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using CompressedImage = RosMessageTypes.Sensor.CompressedImageMsg;
using Image = RosMessageTypes.Sensor.ImageMsg;

namespace ServerConnection.RosTcpConnector
{
    public class RosHeadPositionSubscriber : HeadPositionListenerBase
    {
        //public ROSConnection ros;
        public MeshRenderer meshRenderer;
        public string topic;


        // Start is called before the first frame update
        void Start()
        {
            base.Start();
            Debug.Log("dannyb initializing RosHeadPositionSubscriber");
            ROSConnection.GetOrCreateInstance().Subscribe<JointStateMsg>("/roboy/pinky/control/cardsflow_joint_states", ProcessJointMessage);
        }

        void ProcessJointMessage(JointStateMsg jointState)
        {
            //handle message
            Debug.Log("dannyb process");
            string headAxis0 = "head_axis0";
            string headAxis1 = "head_axis1";
            string headAxis2 = "head_axis2";
            int index_axis0 = Array.IndexOf(jointState.name, headAxis0);
            int index_axis1 = Array.IndexOf(jointState.name, headAxis1);
            int index_axis2 = Array.IndexOf(jointState.name, headAxis2);
            if (index_axis0 >= 0 && index_axis1 >= 0 && index_axis2 >= 0)
            {
                Vector3 headVector = new Vector3((float)jointState.position[0],
                    (float)jointState.position[1],
                    (float)jointState.position[2]);
                Debug.Log("dannyb head :" + headVector.ToString());
                ProcessHeadMessage(headVector);
            }
        }

    }
}