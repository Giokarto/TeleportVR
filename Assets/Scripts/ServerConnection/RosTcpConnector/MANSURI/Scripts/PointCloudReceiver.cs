using System;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace ServerConnection.RosTcpConnector.MANSURI
{
    [Serializable]
    public class DistanceColorPair
    {
        public float distance;
        public Color color;
    }



    public class PointCloudReceiver : MonoBehaviour
    {
       // [SerializeField] private GameObject maskCircleCenter;
        [SerializeField][Range(0f, 300f)] private float maskCircleRadius;
        // Establish a connection with the ROS
        ROSConnection m_Ros;
        // ROS topic from which the Point Cloud Data will be subscribed.
        [SerializeField] public string rosTopicPCL1 = "/left_env_pcd";

        // Serialized color scheme for the point cloud
        [SerializeField]
        public List<DistanceColorPair> colorScheme = new List<DistanceColorPair>()
        {
            new DistanceColorPair { distance = 0.7f, color = Color.red },
            new DistanceColorPair { distance = 0.8f, color = new Color(1f, 0.5f, 0f) },
            new DistanceColorPair { distance = 0.85f, color = new Color(1f, 0.75f, 0f) },
            new DistanceColorPair { distance = 0.9f, color = Color.yellow },
            new DistanceColorPair { distance = 0.95f, color = new Color(0.1f, 0.9f, 1f) },
            new DistanceColorPair { distance = 1f, color = Color.blue },
            new DistanceColorPair { distance = 1.1f, color = new Color(0,0,0,0) }
        };

        // Maximum number of points to render
        [SerializeField] public int maxPoints = 10000;

        // Variables to store and handle the received data.
        private byte[] byteArray;
        private bool isMessageReceived = false;
        private int size;
        // Point cloud positions
        private Vector3[] pcl;
        // Point cloud colors
        private Color[] pcl_color;

        // Metadata about the received Point Cloud.
        private int width;
        private int height;
        private int row_step;
        private int point_step;

        private void Start()
        {
            // Get ROS connection instance and subscribe to the ROS topic
            m_Ros = ROSConnection.instance;
            m_Ros.Subscribe<PointCloud2Msg>(rosTopicPCL1, PointCloudCallback1);
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
        private void PointCloudCallback1(PointCloud2Msg message)
        {
            ReceiveMessage(message, 1);
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

            // Extracting metadata about the received Point Cloud.
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

            pcl = new Vector3[size];
            pcl_color = new Color[size];

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

                pcl[n] = new Vector3(x, z, y);

                // Assign color by distance using color scheme
                pcl_color[n] = ColorByDistance(pcl[n]);
            }
            
        }

        // Assign a color to a point based on its distance from the origin
        private Color ColorByDistance(Vector3 point)
        {
            float dist = point.magnitude;
            //float distCircle = Vector2.Distance(point + transform.position, maskCircleCenter.transform.position);


            foreach (var pair in colorScheme)
            {
                if (dist <= pair.distance)
                {
                    return pair.color;
                }
            }

            return new Color(0, 1, 0, 1);
        }

        // Public method to access the point cloud data.
        public Vector3[] GetPCL()
        {
            return pcl;
        }

        // Public method to access the color of the point cloud data.
        public Color[] GetPCLColor()
        {
            return pcl_color;
        }
        
    }
}
