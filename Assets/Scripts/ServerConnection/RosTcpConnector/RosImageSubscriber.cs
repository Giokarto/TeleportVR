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
    public class RosImageSubscriber : MonoBehaviour
    {
        public string TopicName;

        private bool messageProcessed = false;

        private Texture2D texture2D;

        [SerializeField] private MeshRenderer meshRenderer, secondaryMeshRenderer;

        [SerializeField]
        bool monoVision = false; // if true duplicates the image from meshRenderer to secondaryMeshRenderer

        [SerializeField] bool primary;
        [SerializeField] RosImageSubscriber secondaryImageSubscriber;

        private int frameCount = 0;
        private int receivedCount = 0;
        private float dt = 0.0f;
        public int FPS = 0; // Frames Per Second
        public int RPS = 0; // Received Per Second
        private int updateRate = 1; // 1 update per sec

        private void Start()
        {
            if (meshRenderer == null)
            {
                meshRenderer = ServerData.Instance.RightEye.GetComponentInChildren<MeshRenderer>();
                secondaryMeshRenderer = ServerData.Instance.LeftEye.GetComponentInChildren<MeshRenderer>();
            }
            ROSConnection.GetOrCreateInstance().Subscribe<CompressedImage>(TopicName, GetImage);
            texture2D = new Texture2D(512*2, 512); //, TextureFormat.BGRA32, false);
            auxTexture = new Texture2D(1,1); // auxTexture.LoadImage(ReceivedImage) changes the dimensions according to the image
        }

        private void Update()
        {
            if (messageProcessed)
            {
                meshRenderer.material.mainTexture = texture2D;
                if (monoVision || primary) secondaryMeshRenderer.material.mainTexture = texture2D;
                frameCount++;
                messageProcessed = false;
            }

            UpdateFPS();
        }

        private void GetImage(CompressedImage Message)
        {
            if (!meshRenderer.gameObject.activeInHierarchy) meshRenderer.gameObject.SetActive(true);
            if (monoVision && !secondaryMeshRenderer.gameObject.activeInHierarchy)
                secondaryMeshRenderer.gameObject.SetActive(true);
            receivedCount++;

            if (!messageProcessed)
            {
                StartCoroutine(ProcessImage(Message.data));
            }
        }

        private void UpdateFPS()
        {
            dt += Time.deltaTime;
            if (dt > 1.0f / updateRate)
            {
                FPS = Mathf.RoundToInt(frameCount / dt);
                RPS = Mathf.RoundToInt(receivedCount / dt);
                frameCount = 0;
                receivedCount = 0;
                dt -= 1.0f / updateRate;
            }
        }

        [NonSerialized] public Texture2D auxTexture;
        private IEnumerator ProcessImage(byte[] ReceivedImage)
        {
            // The received image is a square from one eye. We need to:
            // 1. Pad it with the other image, width must be twice as much as height to map correctly to the sphere.
            // 2. Flip it, otherwise the view on the sphere is mirrored.
            
            auxTexture.LoadImage(ReceivedImage);
            if (!(monoVision || primary))
            {
                yield break;
            }

            texture2D.Resize(auxTexture.width * 2, auxTexture.height);
            
            for (int i = 0; i < auxTexture.width; i++)
            {
                for (int j = 0; j < auxTexture.height; j++)
                {
                    if (monoVision)
                    {
                        texture2D.SetPixel(auxTexture.width - 1 - i, j, auxTexture.GetPixel(i, j));
                        texture2D.SetPixel(auxTexture.width - 1 - i + auxTexture.width, j, auxTexture.GetPixel(i, j));
                    }
                    else if (primary)
                    {
                        texture2D.SetPixel(auxTexture.width - 1 - i, j, auxTexture.GetPixel(i, j));
                        texture2D.SetPixel(auxTexture.width - 1 - i + auxTexture.width, j, secondaryImageSubscriber.auxTexture.GetPixel(i, j));
                    }
                }
            }
            texture2D.Apply();
            
            byte[] bytes = auxTexture.EncodeToPNG();
            File.WriteAllBytes("C:\\Users\\Roboy\\projects\\src\\github.com\\Roboy\\image_3.jpg", bytes);
            File.WriteAllBytes("C:\\Users\\Roboy\\projects\\src\\github.com\\Roboy\\image_2.jpg", ReceivedImage);

            meshRenderer.material.SetTexture("_MainTex", texture2D);
            secondaryMeshRenderer.material.SetTexture("_MainTex", texture2D);
            messageProcessed = true;

            yield return null;
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