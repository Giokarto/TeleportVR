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

        // Establish a connection with the ROS
        ROSConnection m_Ros;
        // ROS topic from which the Shoulder Point Cloud Data will be subscribed.
        [SerializeField] public string rosTopicPCL2 = "/left_arm_pcd";
        // Default color for the Point Cloud Data visualization.
        [SerializeField] public Color pclColor = Color.white;

        // Maximum number of points to render
        [SerializeField] public int maxPoints = 10000;

        // Variables to store and handle the received data.
        private byte[] byteArray;
        private bool isMessageReceived = false;
        private int size;
        private Vector3[] shoulderPcl;
        private Color pclShoulderColor;

        // Metadata about the received shoulder Point Cloud.
        private int width;
        private int height;
        private int row_step;
        private int point_step;

        private void Start()
        {
            // On start, subscribe to the specified ROS topic.
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
        // Callback to handle Point Cloud messages from ROS.
        private void PointCloudCallback2(PointCloud2Msg message)
        {
            ReceiveMessage(message, 2);
        }
        // Process the received message and extract relevant data.
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

            // Extracting metadata about the received shoulder Point Cloud.
            width = (int)message.width;
            height = (int)message.height;
            row_step = (int)message.row_step;
            point_step = (int)message.point_step;

            size = size / point_step;
            isMessageReceived = true;
        }

        // Render the received shoulder Point Cloud Data.
        private void PointCloudRendering()
        {
            if (byteArray == null)
            {
                Debug.LogError("No data received from ROS topic");
                return;
            }

            shoulderPcl = new Vector3[size];
            pclShoulderColor = pclColor;
            // Ensure the color is fully opaque
            pclShoulderColor.a = 1f;

            // Debug.Log("Point Cloud Color: " + pclShoulderColor); // Debug log to check the color

            // Extracting position values from the byte array and forming Vector3 points.
            int x_posi, y_posi, z_posi;
            float x, y, z;

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

            // Sort points by their magnitude (distance from origin).
            System.Array.Sort(shoulderPcl, (a, b) => a.magnitude.CompareTo(b.magnitude));

            // Limit the points to the max allowed for rendering.
            if (shoulderPcl.Length > maxPoints)
            {
                shoulderPcl = shoulderPcl.Take(maxPoints).ToArray();
            }
        }
        // Public method to access the shoulder point cloud data.
        public Vector3[] GetShouderPCL()
        {
            return shoulderPcl;
        }

        // Public method to access the color of the shoulder point cloud data.
        public Color GetShoulderPCLColor()
        {
            return pclShoulderColor;
        }
    }
}
