using System;
using UnityEngine;
using UnityEngine.UI;

namespace DisplayExtensions
{
    public class FullScreenOverlay : MonoBehaviour
    {
        private GameObject display;
        private RawImage image;

        private void Awake()
        {
            display = GameObject.Find("Background"); // background from canvas in HUD
            image = display.GetComponent<RawImage>();
            
            // example texture in background, could be e.g. detected faces in the camera stream
            var texture = Resources.Load<Texture2D>("Icons/Cage/CageGreen");
            image.texture = texture;
        }
    }
}