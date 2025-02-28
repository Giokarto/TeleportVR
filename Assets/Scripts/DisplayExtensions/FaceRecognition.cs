using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.UnityUtils;
using ServerConnection;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using Rect = OpenCVForUnity.CoreModule.Rect;

namespace DisplayExtensions
{
    /// <summary>
    /// Face recognition for testing purposes on the Unity side. Is too slow to use in production.
    /// </summary>
    public class FaceRecognition : FullScreenOverlay
    {
        private string filename;
        private CascadeClassifier cascade;
        private MatOfRect faces;
        private Texture2D texture;
        private Mat rgbaMat;
        private Mat grayMat;
        
        private Material mat;
        
        void Start()
        {
            //store name of xml file
            filename = "haarcascade_frontalface_default.xml"; 
            //initaliaze cascade classifier
            cascade = new CascadeClassifier(); 
            //load the xml file data
            cascade.load(Utils.getFilePath(filename)); 
            //initalize faces matofrect
            faces = new MatOfRect();
            //initialize rgb and gray Mats
            rgbaMat = new Mat(1280, 1280, CvType.CV_8UC4);
            grayMat = new Mat(1280, 1280, CvType.CV_8UC4);
 
            //initialize texture2d
            texture = new Texture2D(rgbaMat.cols(), rgbaMat.rows(), TextureFormat.RGBA32, false);

            // add material to the plane where camera stream is rendered; will render face recognition as an overlay
            mat = new Material(Shader.Find("Sprites/Default"));
            leftRenderer.AddMaterial(mat);
        }
        
        void Update()
        {
            currentTexture = GetOverlayTexture();
            
            //set rawimage texture
            this.GetComponent<RawImage>().texture = currentTexture;
            leftRenderer.materials[1].mainTexture = currentTexture;
        }

        public override Texture GetOverlayTexture()
        {
            var rects = GetFaces();
            
            //draw rectangles
            rgbaMat = new Mat(1280, 1280, CvType.CV_8UC4); // clear material without the video image
            for (int i = 0; i < rects.Length; i++)
            {
                Imgproc.rectangle(rgbaMat, new Point(rects[i].x, rects[i].y), new Point(rects[i].x + rects[i].width, rects[i].y + rects[i].height), new Scalar(255, 0, 0, 255), 2);
            }
            
            //convert rgb mat back to texture
            Utils.fastMatToTexture2D(rgbaMat, texture);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Should be call to cloud server which recognizes the faces.
        /// For the purpose of testing compute the faces here.
        /// </summary>
        /// <returns></returns>
        Rect[] GetFaces()
        {
            var textures = ServerBase.Instance.GetVisionTextures();
            
            //convert left texture to rgb mat
            Utils.texture2DToMat(textures[0], rgbaMat);
            
            //convert rgbmat to grayscale
            Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY); 
            
            //extract faces
            cascade.detectMultiScale(grayMat, faces, 1.1, 4); 
            
            //store faces in array
            Rect[] rects = faces.toArray();

            return rects;
        }
    }
}