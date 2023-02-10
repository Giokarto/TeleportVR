using System;
using UnityEngine;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// Frame manipulator class for aiortc connections. It is capable of listening frames, reading pixels and do
    /// transformations such as vertical mirrorings
    /// Currently not in use since it is causing problems on Oculus, can be investigated more
    /// </summary>
    public class ImtpEncoder : MonoBehaviour
    {
        [SerializeField] public GameObject leftEye;
        [SerializeField] public GameObject rightEye;
        [SerializeField] private VideoStreamViewType videoStreamViewType;
        private Renderer leftEyeRenderer, righEyeRenderer;
        private Texture2D lastReceivedTexture;

        public enum VideoStreamViewType
        {
            Single,
            Stereo
        }

        /// <summary>
        /// Sets game object for left and right eye spheres
        /// </summary>
        public void Start()
        {
            leftEyeRenderer = leftEye.GetComponentInChildren<Renderer>();
            righEyeRenderer = rightEye.GetComponentInChildren<Renderer>();
        }

        /// <summary>
        /// Recieves the last frame from AiortcConnector.Update function then updates its own game objects
        /// </summary>
        /// <param name="lastReceivedTexture_">incomming frame texture</param>
        public void SetLastReceivedTexture(Texture lastReceivedTexture_)
        {
            try
            {
                lastReceivedTexture = GetTexture(lastReceivedTexture_);
                UpdateTextures();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Converts incoming Texture to Texture2D
        /// </summary>
        /// <param name="mainTexture">incomming frame texture</param>
        private Texture2D GetTexture(Texture mainTexture)
        {
            return mainTexture as Texture2D;

            // Texture2D tex = mainTexture as Texture2D;
            //
            // var faces = (FindObjectOfType(typeof(AiortcConnector)) as AiortcConnector).faceCoordinates;
            // foreach (var face in faces)
            // {
            //     for (int i = face[0]; i < face[0]+face[2]; i++)
            //     {
            //         for (int j = face[1]; j < face[1]+face[3]; j++)
            //         {
            //             tex.SetPixel(i, tex.height - j, Color.blue);
            //         }
            //     }
            // }
            // tex.Apply();
            //
            // return tex;
        }


        /// <summary>
        /// Manipulates incoming frame by type, if it is Single camera data incoming it manipulates the frames so that each eye sees correctly
        /// if it Stereo it also divides incoming frames and creates correct eye view
        /// also mirrors the frames by default
        /// </summary>
        /// <param name="originalTexture">incomming frame texture</param>
        /// <param name="side">indicates which eye, can be "left" or "right" </param>
        Texture2D GetEyeTexture2D(Texture2D originalTexture, string side)
        {
            var width = videoStreamViewType == VideoStreamViewType.Single
                ? originalTexture.width
                : originalTexture.width / 2;
            var height = originalTexture.height;
            var rightEyeOffset = videoStreamViewType == VideoStreamViewType.Single ? 0 : originalTexture.width / 2 - 1;
            var croppedTexture = new Texture2D(width * 2, height);
            var colors = originalTexture.GetPixels(side == "left" ? 0 : rightEyeOffset, 0, width, height);
            colors = FlipColors(colors, width, height);
            croppedTexture.SetPixels(width, 0, width, height, colors);
            croppedTexture.Apply();
            return croppedTexture;
        }

        /// <summary>
        /// Vertical mirroring of color array
        /// </summary>
        /// <param name="colors">color array that will be mirrored vertically</param>
        /// <param name="width">width of frame</param>
        /// <param name="height">height of frame</param>
        Color[] FlipColors(Color[] colors, int width, int height)
        {
            Color[] newColor = new Color[width * height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    newColor[j * width + i] = colors[j * width + width - i - 1];
                }
            }

            return newColor;
        }

        /// <summary>
        /// Updates eye game object textures
        /// </summary>
        private void UpdateTextures()
        {
            leftEyeRenderer.material.mainTexture = GetEyeTexture2D(lastReceivedTexture, "left");
            righEyeRenderer.material.mainTexture = GetEyeTexture2D(lastReceivedTexture, "right");
        }
    }
}
