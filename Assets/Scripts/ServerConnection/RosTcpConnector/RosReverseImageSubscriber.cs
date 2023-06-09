using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.IO;
using UnityEngine.UI;

namespace ServerConnection.RosTcpConnector
{
    public class RosReverseImageSubscriber : MonoBehaviour
    {
        [SerializeField] private string leftTopicName = "/image/left/image_compressed";
        [SerializeField] private string rightTopicName = "/image/right/image_compressed";
        [SerializeField] private MeshRenderer leftMeshRenderer;
        [SerializeField] private MeshRenderer rightMeshRenderer;
        [SerializeField] public float fallbackTime = 5.0f;
        [SerializeField] private RawImage leftRawImage;
        [SerializeField] private RawImage rightRawImage;

        private Texture2D texture2DL;
        private Texture2D texture2DR;
        private bool newImages = false;
        private int receivedCountL = 0;
        private int receivedCountR = 0;
        public float lastReceivedL = -1000;
        public float lastReceivedR = -1000;
        private int frameCount = 0;
        private float dt = 0.0f;
        public int FPS = 0;
        public int RPSL = 0;
        public int RPSR = 0;

        private void Start()
        {
            if (leftMeshRenderer == null)
                leftMeshRenderer = ServerBase.Instance.LeftEye.GetComponentInChildren<MeshRenderer>();

            if (rightMeshRenderer == null)
                rightMeshRenderer = ServerBase.Instance.RightEye.GetComponentInChildren<MeshRenderer>();

            texture2DL = new Texture2D(2, 2, TextureFormat.RGB24, false);
            texture2DR = new Texture2D(2, 2, TextureFormat.RGB24, false);

            ROSConnection.GetOrCreateInstance().Subscribe<CompressedImageMsg>(leftTopicName, GetImageL);
            ROSConnection.GetOrCreateInstance().Subscribe<CompressedImageMsg>(rightTopicName, GetImageR);

            // Add debug log statements
            Debug.Log("Subscribed to left topic: " + leftTopicName);
            Debug.Log("Subscribed to right topic: " + rightTopicName);
        }

        private void Update()
        {
            if (newImages)
            {
                UpdateMeshRenderers();
                UpdateFPS();

                receivedCountL = 0;
                receivedCountR = 0;
                newImages = false;
            }
        }

        private void UpdateMeshRenderers()
        {
            if (receivedCountL > 0)
            {
                leftMeshRenderer.material.mainTexture = texture2DL;
                if (Time.time - lastReceivedR > fallbackTime)
                    rightMeshRenderer.material.mainTexture = texture2DL;
            }

            if (receivedCountR > 0)
            {
                rightMeshRenderer.material.mainTexture = texture2DR;
                if (Time.time - lastReceivedL > fallbackTime)
                    leftMeshRenderer.material.mainTexture = texture2DR;
            }
        }

        private void GetImageL(CompressedImageMsg message)
        {
            receivedCountL++;
            lastReceivedL = Time.time;
            byte[] imageData = message.data;
            texture2DL.LoadImage(imageData);
            texture2DL.Apply();
            newImages = true;

            // Add debug log statements
            Debug.Log("Received image on left topic: " + leftTopicName);
            Debug.Log("Received image data length: " + imageData.Length);
        }

        private void GetImageR(CompressedImageMsg message)
        {
            receivedCountR++;
            lastReceivedR = Time.time;
            byte[] imageData = message.data;
            texture2DR.LoadImage(imageData);
            texture2DR.Apply();
            newImages = true;

            // Add debug log statements
            Debug.Log("Received image on right topic: " + rightTopicName);
            Debug.Log("Received image data length: " + imageData.Length);
        }

        private void UpdateFPS()
        {
            dt += Time.deltaTime;
            if (dt > 1.0f)
            {
                FPS = frameCount;
                RPSL = receivedCountL;
                RPSR = receivedCountR;
                frameCount = 0;
                dt -= 1.0f;

                // Add debug log statement
                Debug.Log("FPS: " + FPS + ", RPSL: " + RPSL + ", RPSR: " + RPSR);
            }
            frameCount++;
        }
    }
}
