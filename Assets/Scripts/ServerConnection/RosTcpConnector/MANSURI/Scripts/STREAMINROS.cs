using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

namespace ServerConnection.RosTcpConnector.MANSURI
{
    public class STREAMINROS : MonoBehaviour
    {
        // Adjust this value to increase or decrease brightness of depth images
        [SerializeField] float brightnessFactor = 8f;

        // Textures that will store the received images from different ROS topics.
        Texture2D texRos;
        Texture2D texRos2;
        Texture2D texRos3;
        Texture2D texRos4;

        // UI elements to display the received images.
        public RawImage display;
        public RawImage display2;
        public RawImage display3;
        public RawImage display4;

        // Instance of the ROS connection.
        ROSConnection m_Ros;
        ImageMsg img_msg;

        // ROS topics from which images will be subscribed.
        [SerializeField] public string rosTopic1 = "/camera_shoulder_left/color/image_raw";
        [SerializeField] public string rosTopic2 = "/warn_image_back";
        [SerializeField] public string rosTopic3 = "/camera_back/depth/image_rect_raw";
        [SerializeField] public string rosTopic4 = "/camera_shoulder_left/depth/image_rect_raw";
        [SerializeField] public string rosDistanceTopic = "/obstacle_distance";

        void Start()
        {
            // Initializing the ROS connection.
            m_Ros = ROSConnection.GetOrCreateInstance();
            // Subscribing to different ROS topics.
            m_Ros.Subscribe<ImageMsg>(rosTopic1, RGB_cam1);
            m_Ros.Subscribe<ImageMsg>(rosTopic2, RGB_cam2);
            m_Ros.Subscribe<ImageMsg>(rosTopic3, Depth_cam1);
            m_Ros.Subscribe<ImageMsg>(rosTopic4, Depth_cam2);

            // Debug.Log("Subscribed to  topic: " + rosTopic1);
            // Debug.Log("Subscribed to  topic: " + rosTopic2);
            // Debug.Log("Subscribed to  topic: " + rosTopic3);
            // Debug.Log("Subscribed to  topic: " + rosTopic4);

        }

        // Callback to process RGB images for camera 1.
        public void RGB_cam1(ImageMsg img)
        {
            texRos = new Texture2D((int)img.width, (int)img.height, TextureFormat.RGB24, false);
            texRos.LoadRawTextureData(img.data);
            texRos.Apply();
            display.texture = texRos;

            // Debug.Log("Depth image received from " + rosTopic1);
            //Debug.Log("Data transferred: " + img.data.Length + " bytes");
        }

        // Callback to process RGB images for camera 2.
        public void RGB_cam2(ImageMsg img)
        {
            texRos2 = new Texture2D((int)img.width, (int)img.height, TextureFormat.RGB24, false);

            texRos2.LoadRawTextureData(img.data);
            texRos2.Apply();
            display2.texture = texRos2;

            // Debug.Log("Depth image received from " + rosTopic2);
            // Debug.Log("Data transferred: " + img.data.Length + " bytes");
        }

        // Callback to process depth images for camera 1.
        public void Depth_cam1(ImageMsg img)
        {
            // Process the depth image and convert it to grayscale based on depth values.
            texRos3 = new Texture2D((int)img.width, (int)img.height, TextureFormat.R16, false);
            texRos3.LoadRawTextureData(img.data);
            texRos3.Apply();

            Texture2D texGrayscale = new Texture2D((int)img.width, (int)img.height, TextureFormat.RGB24, false);
            for (int y = 0; y < texRos3.height; y++)
            {
                for (int x = 0; x < texRos3.width; x++)
                {
                    float gray = texRos3.GetPixel(x, y).r * brightnessFactor;
                    // Ensure the value stays within the valid range
                    gray = Mathf.Clamp01(gray);
                    texGrayscale.SetPixel(x, y, new Color(gray, gray, gray));
                }
            }
            texGrayscale.Apply();

            display3.texture = texGrayscale;

            // Debug.Log("Depth image received from " + rosTopic3);
            // Debug.Log("Data transferred: " + img.data.Length + " bytes");
        }

        // Callback to process depth images for camera 2.
        public void Depth_cam2(ImageMsg img)
        {
            // Process the depth image and convert it to grayscale based on depth values.
            texRos4 = new Texture2D((int)img.width, (int)img.height, TextureFormat.R16, false);
            texRos4.LoadRawTextureData(img.data);
            texRos4.Apply();

            Texture2D texGrayscale = new Texture2D((int)img.width, (int)img.height, TextureFormat.RGB24, false);
            for (int y = 0; y < texRos4.height; y++)
            {
                for (int x = 0; x < texRos4.width; x++)
                {
                    float gray = texRos4.GetPixel(x, y).r * brightnessFactor;
                    // Ensure the value stays within the valid range
                    gray = Mathf.Clamp01(gray);
                    texGrayscale.SetPixel(x, y, new Color(gray, gray, gray));
                }
            }
            texGrayscale.Apply();

            display4.texture = texGrayscale;

            // Debug.Log("Depth image received from " + rosTopic4);
            // Debug.Log("Data transferred: " + img.data.Length + " bytes");
        }
    }
}