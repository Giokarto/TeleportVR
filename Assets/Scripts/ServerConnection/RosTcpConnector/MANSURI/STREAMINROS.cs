using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

namespace ServerConnection.RosTcpConnector
{
    public class STREAMINROS : MonoBehaviour
    {
        // Adjust this value to increase or decrease brightness
        [SerializeField] float brightnessFactor = 8f;
        
        
        Texture2D texRos;
        Texture2D texRos2;
        Texture2D texRos3;
        Texture2D texRos4;

        public RawImage display;
        public RawImage display2;
        public RawImage display3;
        public RawImage display4;
        //public RawImage display5;


        //public GameObject barrierPrefab;  // Assign your barrier prefab in the inspector
       // private GameObject currentBarrier;  // The current barrier instance
        
        ROSConnection m_Ros;
        ImageMsg img_msg;

        [SerializeField] public string rosTopic1 = "/camera_shoulder_left/color/image_raw";
        [SerializeField] public string rosTopic2 = "/warn_image_back";
        [SerializeField] public string rosTopic3 = "/camera_back/depth/image_rect_raw";
        [SerializeField] public string rosTopic4 = "/camera_shoulder_left/depth/image_rect_raw";
        //[SerializeField] public string rosTopic5 = "/camera1/depth/color/points";

        //[SerializeField] public string rosDistanceTopic = "/obstacle_distance";
        
        void Start()
        {
            m_Ros = ROSConnection.GetOrCreateInstance();
            Debug.Log(m_Ros.RosIPAddress);
            m_Ros.Subscribe<ImageMsg>(rosTopic1, RGB_cam1);
            m_Ros.Subscribe<ImageMsg>(rosTopic2, RGB_cam2);
            m_Ros.Subscribe<ImageMsg>(rosTopic3, Depth_cam1);
           m_Ros.Subscribe<ImageMsg>(rosTopic4, Depth_cam2);
           
          // m_Ros.Subscribe<Float64Msg>(rosDistanceTopic, HandleDistanceMessage);
           
           
            Debug.Log("Subscribed to right topic: " + rosTopic1);
            Debug.Log("Subscribed to right topic: " + rosTopic2);
            Debug.Log("Subscribed to right topic: " + rosTopic3);
            Debug.Log("Subscribed to right topic: " + rosTopic4);
            //Debug.Log("Subscribed to right topic - pointclouds*STREAMINROS: " + rosTopic5);

            //Debug.Log("Subscribed to distance topic: " + rosDistanceTopic);
        }

        public void RGB_cam1(ImageMsg img)
        {
            texRos = new Texture2D((int)img.width, (int)img.height, TextureFormat.RGB24, false);
            texRos.LoadRawTextureData(img.data);
            texRos.Apply();
            display.texture = texRos;

            Debug.Log("Depth image received from " + rosTopic1);
            Debug.Log("Data transferred: " + img.data.Length + " bytes");
        }

        public void RGB_cam2(ImageMsg img)
        {
            texRos2 = new Texture2D((int)img.width, (int)img.height, TextureFormat.RGB24, false);

            /*
            // Swap red and blue channels
            byte[] correctedData = new byte[img.data.Length];
            for (int i = 0; i < img.data.Length; i += 3)
            {
                correctedData[i] = img.data[i + 2];  // Blue channel
                correctedData[i + 1] = img.data[i + 1];  // Green channel
                correctedData[i + 2] = img.data[i];  // Red channel
            }*/
            
            texRos2.LoadRawTextureData(img.data);
            texRos2.Apply();
            display2.texture = texRos2;

            Debug.Log("Depth image received from " + rosTopic2);
            Debug.Log("Data transferred: " + img.data.Length + " bytes");
        }

        public void Depth_cam1(ImageMsg img)
        {
            texRos3 = new Texture2D((int)img.width, (int)img.height, TextureFormat.R16, false);
            texRos3.LoadRawTextureData(img.data);
            texRos3.Apply();

            Texture2D texGrayscale = new Texture2D((int)img.width, (int)img.height, TextureFormat.RGB24, false);
            for (int y = 0; y < texRos3.height; y++)
            {
                for (int x = 0; x < texRos3.width; x++)
                {
                    float gray = texRos3.GetPixel(x, y).r * brightnessFactor;
                    gray = Mathf.Clamp01(gray); // Ensure the value stays within the valid range
                    texGrayscale.SetPixel(x, y, new Color(gray, gray, gray));
                }
            }
            texGrayscale.Apply();

            display3.texture = texGrayscale;

            Debug.Log("Depth image received from " + rosTopic3);
            Debug.Log("Data transferred: " + img.data.Length + " bytes");
        }

        public void Depth_cam2(ImageMsg img)
        {
            texRos4 = new Texture2D((int)img.width, (int)img.height, TextureFormat.R16, false);
            texRos4.LoadRawTextureData(img.data);
            texRos4.Apply();

            Texture2D texGrayscale = new Texture2D((int)img.width, (int)img.height, TextureFormat.RGB24, false);
            for (int y = 0; y < texRos4.height; y++)
            {
                for (int x = 0; x < texRos4.width; x++)
                {
                    float gray = texRos4.GetPixel(x, y).r * brightnessFactor;
                    gray = Mathf.Clamp01(gray); // Ensure the value stays within the valid range
                    texGrayscale.SetPixel(x, y, new Color(gray, gray, gray));
                }
            }
            texGrayscale.Apply();

            display4.texture = texGrayscale;

            Debug.Log("Depth image received from " + rosTopic4);
            Debug.Log("Data transferred: " + img.data.Length + " bytes");
        }

        
        
       
       
      /* 
       public void HandleDistanceMessage(Float64Msg msg)
       {
           double distance = msg.data;

           Debug.Log(" distance Data transferred: " + msg.data + " mm");

           // If the distance is less than 20cm (or 0.2m in Unity units)
           if (distance < 0.2f)
           {
               // If there's not already a barrier, create one
               if (currentBarrier == null)
               {
                   currentBarrier = Instantiate(barrierPrefab, transform.position, Quaternion.identity);
               }
                
               Debug.Log(" distance Data transferred : " + msg.data + "distance < 0.2f" );

           }
           else  // If the distance is greater than 20cm and a barrier exists, destroy it
           {
               if (currentBarrier != null)
               {
                   Destroy(currentBarrier);
                   currentBarrier = null;
               }
                
               Debug.Log(" distance Data transferred: " + msg.data + currentBarrier != null + " mm");

           }

            
       }
       */
    }
}