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
        private Vector3[] shoulderPcl;
        private Color pclShoulderColor;
        private int width;
        private int height;
        private int row_step;
        private int point_step;

        private void Start()
        {
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

        private void PointCloudCallback2(PointCloud2Msg message)
        {
            ReceiveMessage(message, 2);
        }

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
        }

        private void PointCloudRendering()
        {
            if (byteArray == null)
            {
                Debug.LogError("No data received from ROS topic");
                return;
            }

            shoulderPcl = new Vector3[size];
            pclShoulderColor = pclColor;
            pclShoulderColor.a = 1f;  // Ensure the color is fully opaque

            Debug.Log("Point Cloud Color: " + pclShoulderColor); // Debug log to check the color

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

            System.Array.Sort(shoulderPcl, (a, b) => a.magnitude.CompareTo(b.magnitude));

            if (shoulderPcl.Length > maxPoints)
            {
                shoulderPcl = shoulderPcl.Take(maxPoints).ToArray();
            }
        }

        public Vector3[] GetShouderPCL()
        {
            return shoulderPcl;
        }

        public Color GetShoulderPCLColor()
        {
            return pclShoulderColor;
        }
    }
}
