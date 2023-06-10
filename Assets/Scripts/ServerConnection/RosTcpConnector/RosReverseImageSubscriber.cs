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
        [SerializeField] private string leftTopicName = "/camera1/color/image_raw";
        [SerializeField] private string rightTopicName ="/camera2/color/image_raw";
        [SerializeField] private MeshRenderer leftMeshRenderer;
        [SerializeField] private MeshRenderer rightMeshRenderer;
        [SerializeField] public float monoFallbackTime = 5.0f;
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
        private int updateRate = 1; // 1 update per sec

        private void Start()
        {
            if (leftMeshRenderer == null)
                leftMeshRenderer = ServerBase.Instance.LeftReverseEye.GetComponentInChildren<MeshRenderer>();

            if (rightMeshRenderer == null)
                rightMeshRenderer = ServerBase.Instance.RightReverseEye.GetComponentInChildren<MeshRenderer>();

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
                if (!leftMeshRenderer.gameObject.activeInHierarchy) leftMeshRenderer.gameObject.SetActive(true);
                if (!rightMeshRenderer.gameObject.activeInHierarchy) rightMeshRenderer.gameObject.SetActive(true);

                if (receivedCountL > 0)
                {
                    leftMeshRenderer.material.mainTexture = texture2DL;
                    if (Time.time - lastReceivedR > monoFallbackTime)
                        rightMeshRenderer.material.mainTexture = texture2DL;
                }

                if (receivedCountR > 0)
                {
                    rightMeshRenderer.material.mainTexture = texture2DR;
                    if (Time.time - lastReceivedL > monoFallbackTime)
                        leftMeshRenderer.material.mainTexture = texture2DR;
                }

                frameCount++;
                newImages = false;
            }

            UpdateFPS();

            receivedCountL = 0;
            receivedCountR = 0;
        }


        private void GetImageL(CompressedImageMsg message)
        {
            receivedCountL++;
            lastReceivedL = Time.time;
            byte[] imageData = message.data;
            texture2DL.LoadImage(imageData);
            texture2DL.Apply();
            newImages = true;
        }

        private void GetImageR(CompressedImageMsg message)
        {
            receivedCountR++;
            lastReceivedR = Time.time;
            byte[] imageData = message.data;
            texture2DR.LoadImage(imageData);
            texture2DR.Apply();
            newImages = true;
        }


        private void UpdateFPS()
        {
            dt += Time.deltaTime;
            if (dt > 1.0f / updateRate)
            {
                FPS = Mathf.RoundToInt(frameCount / dt);
                RPSL = Mathf.RoundToInt(receivedCountL / dt);
                RPSR = Mathf.RoundToInt(receivedCountR / dt);
                frameCount = 0;
                dt -= 1.0f / updateRate;

                Debug.Log("FPS: " + FPS + ", RPSL: " + RPSL + ", RPSR: " + RPSR);
            }
            frameCount++;
        }

    }
}
