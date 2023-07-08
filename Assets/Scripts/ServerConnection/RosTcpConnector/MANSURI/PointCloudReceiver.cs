using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

namespace ServerConnection.RosTcpConnector
{
    public class PointCloudReceiver : MonoBehaviour
    {
        ROSConnection m_Ros;
        [SerializeField] public string rosTopic1 = "/camera1/depth/color/points";

        private byte[] byteArray;
        private bool isMessageReceived = false;
        private int size;
        private Vector3[] pcl;
        private Color[] pcl_color;
        private int width;
        private int height;
        private int row_step;
        private int point_step;


       static Color orange = new Color(1.0f, 0.647f, 0.0f);  // define orange
        private Color[] colorSchema = { Color.red, orange, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };

        private void Start()
        {
            m_Ros = ROSConnection.instance;
            m_Ros.Subscribe<PointCloud2Msg>(rosTopic1, PointCloudCallback1);
            Debug.Log("Subscribed to point cloud topic: " + rosTopic1);
        }

        private void Update()
        {
            if (isMessageReceived)
            {
                PointCloudRendering();
                isMessageReceived = false;
            }
        }

        private void PointCloudCallback1(PointCloud2Msg message)
        {
            ReceiveMessage(message);
        }

        private void ReceiveMessage(PointCloud2Msg message)
        {
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
        }

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
