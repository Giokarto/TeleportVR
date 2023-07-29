using System;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace ServerConnection.RosTcpConnector.MANSURI
{
    public class ShoulderPCLreceiver : MonoBehaviour
    {
        ROSConnection m_Ros;
        [SerializeField] public string rosTopicPCL2 = "/left_arm_pcd";
        [SerializeField] public Color pclColor = Color.white;

        // Maximum number of points to render
        [SerializeField] public int maxPoints = 10000;

        private byte[] byteArray;
        private bool isMessageReceived = false;
        private int size;
        // Point cloud positions
        private Vector3[] shoulderPcl;
        // Point cloud color
        private Color pclShoulderColor;
        // Width of the point cloud
        private int width;
        // Height of the point cloud
        private int height;
        // Row step of the point cloud
        private int row_step;
        // Point step of the point cloud
        private int point_step;

        private void Start()
        {
            // Get ROS connection instance and subscribe to the ROS topic
            m_Ros = ROSConnection.instance;
            m_Ros.Subscribe<PointCloud2Msg>(rosTopicPCL2, PointCloudCallback2);
        }

        private void Update()
        {
            if (isMessageReceived)
            {
                PointCloudRendering();
                isMessageReceived = false;
            }
        }

        // Callback for when a message is received from the ROS topic
        private void PointCloudCallback2(PointCloud2Msg message)
        {
            ReceiveMessage(message, 2);
        }

        // Process the received message
        private void ReceiveMessage(PointCloud2Msg message, int camera)
        {
            if (message.data == null)
            {
                Debug.LogError("Received null data from ROS topic");
                return;
            }

            size = message.data.Length;
            byteArray = new byte[size];
            byteArray = message.data;

            width = (int)message.width;
            height = (int)message.height;
            row_step = (int)message.row_step;
            point_step = (int)message.point_step;

            size = size / point_step;
            isMessageReceived = true;
            // If this is the second camera, apply a transformation to the points
            if (camera == 2)
            {
                // Define the rotation and translation that represent the relative position and orientation of the two cameras
                Quaternion rotation = Quaternion.Euler(0, 180, 0);  // Replace with the actual rotation
                Vector3 translation = new Vector3(0, 0, 0);  // Replace with the actual translation

                for (int i = 0; i < shoulderPcl.Length; i++)
                {
                    // Apply the rotation and translation to each point
                    shoulderPcl[i] = rotation * shoulderPcl[i] + translation;
                }
            }
        }

        private void PointCloudRendering()
        {
            if (byteArray == null)
            {
                Debug.LogError("No data received from ROS topic");
                return;
            }

            shoulderPcl = new Vector3[size];
            pclShoulderColor = pclColor; // Set the single color for the entire point cloud

            int x_posi;
            int y_posi;
            int z_posi;

            float x;
            float y;
            float z;

            for (int n = 0; n < size; n++)
            {
                x_posi = n * point_step + 0;
                y_posi = n * point_step + 4;
                z_posi = n * point_step + 8;

                x = System.BitConverter.ToSingle(byteArray, x_posi);
                y = System.BitConverter.ToSingle(byteArray, y_posi);
                z = System.BitConverter.ToSingle(byteArray, z_posi);

                shoulderPcl[n] = new Vector3(x, z, y);
            }

            // Sort points by distance
            System.Array.Sort(shoulderPcl, (a, b) => a.magnitude.CompareTo(b.magnitude));

            // Only take the closest maxPoints points
            if (shoulderPcl.Length > maxPoints)
            {
                shoulderPcl = shoulderPcl.Take(maxPoints).ToArray();
            }
        }

        public  Vector3[] GetShouderPCL()
        {
            return shoulderPcl;
        }

        public  Color GetShoulderPCLColor()
        {
            return pclShoulderColor;
        }
    }
}
