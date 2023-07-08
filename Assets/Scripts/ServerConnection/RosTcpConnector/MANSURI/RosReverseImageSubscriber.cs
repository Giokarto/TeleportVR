using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using CompressedImage = RosMessageTypes.Sensor.CompressedImageMsg;
using Image = RosMessageTypes.Sensor.ImageMsg;

namespace ServerConnection.RosTcpConnector
{
    public class RosReverseImageSubscriber : MonoBehaviour
    {
        // /image/left/image_compressed
        
        [SerializeField] bool subscribeLeft;
        [SerializeField] public string LeftTopicName;
        [SerializeField] bool subscribeRight;
        [SerializeField] public string RightTopicName;

        private bool newImages = false;

        private Texture2D texture2DL, texture2DR;

        [SerializeField] private MeshRenderer leftMeshRenderer, rightMeshRenderer;

        //[SerializeField]
        //bool monoVision = false; // if true duplicates the image from meshRenderer to secondaryMeshRenderer
        
        
        [SerializeField] public float monoFallbackTime = 5.0f; // Time after which rendering falls back to mono if no images have been received for both eyes

        //[SerializeField] bool primary;
        //[SerializeField] RosImageSubscriber secondaryImageSubscriber;

        private int frameCount = 0;
        private int receivedCountL = 0, receivedCountR = 0;
        public float lastReceivedL = -1000, lastReceivedR = -1000;
        private float dt = 0.0f;
        public int FPS = 0; // Frames Per Second
        public int RPSL = 0, RPSR = 0; // Received Per Second
        private int updateRate = 1; // 1 update per sec

        private void Start()
        {
            if (leftMeshRenderer == null)
                leftMeshRenderer = ServerBase.Instance.LeftEye.GetComponentInChildren<MeshRenderer>();

            if (rightMeshRenderer == null)
                rightMeshRenderer = ServerBase.Instance.RightEye.GetComponentInChildren<MeshRenderer>();

            if (subscribeLeft && LeftTopicName != "")
                ROSConnection.GetOrCreateInstance().Subscribe<Image>(LeftTopicName, GetImageL);
            if (subscribeRight && RightTopicName != "")
                ROSConnection.GetOrCreateInstance().Subscribe<Image>(RightTopicName, GetImageR);
            
            texture2DL = new Texture2D(1024, 1024);
            texture2DR = new Texture2D(1024, 1024); //, TextureFormat.BGRA32, false);
            //auxTexture = new Texture2D(1,1); // auxTexture.LoadImage(ReceivedImage) changes the dimensions according to the image
            
            
            
            // Add debug log statements
            Debug.Log("Subscribed to left topic: " + LeftTopicName);
            Debug.Log("Subscribed to right topic: " + RightTopicName);
        }

        private void Update()
        {
            if (newImages)
            {
                if (!leftMeshRenderer.gameObject.activeInHierarchy) leftMeshRenderer.gameObject.SetActive(true);
                if (!rightMeshRenderer.gameObject.activeInHierarchy) rightMeshRenderer.gameObject.SetActive(true);

                // Put correct images to the corresponding hemispheres,
                // but if images for one side are too old, fall back to mono
                // and assign that image to both eyes
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
        }

        private void GetImageL(Image Message)
        {
            receivedCountL++;
            lastReceivedL = Time.time;
            texture2DL.LoadImage(Message.data);
            texture2DL.Apply();
            newImages = true;
        }
        
        private void GetImageR(Image Message)
        {
            receivedCountR++;
            lastReceivedR = Time.time;
            texture2DR.LoadImage(Message.data);
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
                receivedCountL = 0;
                receivedCountR = 0;
                dt -= 1.0f / updateRate;
            }
            Debug.Log("FPS: " + FPS + ", RPSL: " + RPSL + ", RPSR: " + RPSR);
        }
    }

//public class SubscriberCamera : MonoBehaviour
//{
//    //public ROSConnection ros;
//    public MeshRenderer meshRenderer;
//    public string topic;

//    private Texture2D texture2D;
//    private byte[] imageData;
//    private bool isMessageReceived;
//    // Start is called before the first frame update
//    void Start()
//    {
//        ROSConnection.GetOrCreateInstance().Subscribe<CompressedImage>(topic, ShowImage);
//        texture2D = new Texture2D(960,540, TextureFormat.RGBA32, false);
//        //meshRenderer.material = new Material(Shader.Find("Standard"));
//    }

//    private void Update()
//    {
//        if (isMessageReceived)
//            ProcessImage();
//    }

//    void ShowImage(CompressedImage ImgMsg)
//    {
//        imageData = ImgMsg.data;
//        isMessageReceived = true;
//        //Debug.Log(ImgMsg.format);
//    }

//    void ProcessImage()
//    {
//        texture2D.LoadImage(imageData);
//        //texture2D.Apply();
//        meshRenderer.material.SetTexture("_MainTex", texture2D);
//        isMessageReceived = false;
//    }
//}



//using System.Collections;
//using System.IO;
//using UnityEngine;
//using Unity.Robotics.ROSTCPConnector;
////using RosMessageTypes.Sensor;
//using CompressedImage = RosMessageTypes.Sensor.CompressedImageMsg;

//public class SubscriberCamera : ImageDefaultVisualizer
//{
//    public string TopicName= "/roboy/sensing/camera/left";


//    private bool messageProcessed = false;
//    private Texture2D texture2D;
//    [SerializeField] private MeshRenderer meshRenderer;

//    private int frameCount = 0;
//    private int receivedCount = 0;
//    private float dt = 0.0f;
//    public int FPS = 0;         // Frames Per Second
//    public int RPS = 0;         // Received Per Second
//    public int T = 0;
//    private int updateRate = 1; // 1 update per sec

//    private void Start()
//    {
//        texture2D = new Texture2D(540, 960);
//        meshRenderer.material = new Material(Shader.Find("Standard"));

//        ROSConnection.instance.Subscribe<CompressedImage>(TopicName, GetImage);
//    }

//    private void Update()
//    {

//        if (messageProcessed)
//        {
//            meshRenderer.material.mainTexture =  texture2D;
//            frameCount++;
//            messageProcessed = false;
//        }

//        UpdateFPS();
//    }

//    private void GetImage(CompressedImage Message)
//    {
//        receivedCount++;

//        if (!messageProcessed)
//        {
//            StartCoroutine(ProcessImage(Message.data));
//        }
//    }

//    private void UpdateFPS()
//    {
//        dt += Time.deltaTime;
//        if (dt > 1.0f / updateRate)
//        {
//            FPS = Mathf.RoundToInt(frameCount / dt);
//            RPS = Mathf.RoundToInt(receivedCount / dt);
//            frameCount = 0;
//            receivedCount = 0;
//            dt -= 1.0f / updateRate;
//        }
//    }

//    private IEnumerator ProcessImage(byte[] ReceivedImage)
//    {
//        texture2D.LoadImage(ReceivedImage);
//        texture2D.Apply();

//        meshRenderer.material.SetTexture("_MainTex", texture2D);
//        messageProcessed = true;

//        yield return null;
//    }
//}
}