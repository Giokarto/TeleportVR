using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using System.Linq;

namespace ServerConnection.RosTcpConnector
{
    public class PointCloudReceiver : MonoBehaviour
    {
        ROSConnection m_Ros;
        [SerializeField] public string rosTopic1 = "/camera1/depth/color/points";
        [SerializeField] public string rosTopic2 = "/camera2/depth/color/points";

        // Color scheme for the point cloud, can be changed in the inspector 
        [SerializeField] public Color[] colorSchema = { Color.red, Color.yellow, Color.green, Color.cyan, Color.blue };
        // Maximum number of points to render
        [SerializeField] public int maxPoints = 10000;  

        private byte[] byteArray;
        private bool isMessageReceived = false;
        private int size;
        // Point cloud positions
        private Vector3[] pcl;
        // Point cloud colors
        private Color[] pcl_color;
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
            m_Ros.Subscribe<PointCloud2Msg>(rosTopic1, PointCloudCallback1);
            m_Ros.Subscribe<PointCloud2Msg>(rosTopic2, PointCloudCallback2);
            Debug.Log("Subscribed to point cloud topics: " + rosTopic1 + ", " + rosTopic2);
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

                for (int i = 0; i < pcl.Length; i++)
                {
                    // Apply the rotation and translation to each point
                    pcl[i] = rotation * pcl[i] + translation;
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

            pcl = new Vector3[size];
            pcl_color = new Color[size];

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

                pcl[n] = new Vector3(x, z, y);

                // Assign color by distance using color schema
                pcl_color[n] = ColorByDistance(pcl[n]);
            }

            // Sort points by distance
            System.Array.Sort(pcl, (a, b) => a.magnitude.CompareTo(b.magnitude));

            // Only take the closest maxPoints points
            if (pcl.Length > maxPoints)
            {
                pcl = pcl.Take(maxPoints).ToArray();
                pcl_color = pcl_color.Take(maxPoints).ToArray();
            }
        }

        // Assign a color to a point based on its distance from the origin
        private Color ColorByDistance(Vector3 point)
        {
            float dist = point.magnitude;

            float min_dist = 0.0f;
            float max_dist = 10.0f;

            int colorIndex = Mathf.FloorToInt(((dist - min_dist) / (max_dist - min_dist)) * colorSchema.Length);
            colorIndex = Mathf.Clamp(colorIndex, 0, colorSchema.Length - 1);

            return colorSchema[colorIndex];
        }

        public Vector3[] GetPCL()
        {
            return pcl;
        }

        public Color[] GetPCLColor()
        {
            return pcl_color;
        }
    }
}
