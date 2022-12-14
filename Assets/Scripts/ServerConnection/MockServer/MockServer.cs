using System;
using System.Collections.Generic;
using System.Linq;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;
using UnityEngine.Video;
using Random = System.Random;

namespace ServerConnection.MockServer
{
    /// <summary>
    /// This class mocks a server connection. It displays an Obama video instead of real-world stream.
    /// </summary>
    public class MockServer : ServerData
    {
        private Random random = new Random();

        private VideoPlayer videoPlayer;
        private int fps = 10;

        
        private Texture2D _leftTexture;
        private Texture2D _rightTexture;
        [SerializeField] private GameObject _leftPlane;
        [SerializeField] private GameObject _rightPlane;
        private Renderer _leftRenderer;
        private Renderer _rightRenderer;
        
        
        public void Start()
        {
            ConnectedToServer = false;
            
            videoPlayer = new GameObject("mockVideo", typeof(VideoPlayer)).GetComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.url = "C:\\Users\\Roboy\\projects\\src\\github.com\\Roboy\\ObamaVideoTest.mp4";
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
                
            _leftRenderer = _leftPlane.GetComponent<Renderer>();
            _rightRenderer = _rightPlane.GetComponent<Renderer>();
        }
        
        public void Update()
        {
            ConnectedToServer = random.NextDouble() < 0.98 ? ConnectedToServer : !ConnectedToServer;

            RenderTexture videoTexture = (RenderTexture)videoPlayer.texture;
            
            Texture2D convertedTexture = new Texture2D(videoTexture.width, videoTexture.width, TextureFormat.ARGB32, false);
            RenderTexture.active = videoTexture;
            convertedTexture.ReadPixels(new UnityEngine.Rect(0, 0, videoTexture.width, videoTexture.height), 0, 0);
            convertedTexture.Apply();
            
            _leftTexture = convertedTexture;
            
            _leftRenderer.material.mainTexture = _leftTexture;
            _rightRenderer.material.mainTexture = _leftTexture;
        }

        public override bool ConnectedToServer { get; protected set; }

        public override Dictionary<Modality, bool> ModalityConnected { get; }
            = Enum.GetValues(typeof(Modality)).Cast<Enum>().ToDictionary(e => (Modality)e, v => false);

        public override bool EnableVision(bool stereo)
        {
            return true;
        }

        public override void DisableVision() {}

        public override float GetVisionLatency()
        {
            return random.Next(10, 100);
        }

        public override float GetVisionFps()
        {
            return fps;
        }

        public override Texture2D[] GetVisionTextures()
        {
            return new[] { _leftTexture, _leftTexture }; // only mono vision here
        }

        protected override void SetPresenceIndicatorOn(bool on) {}
        public override void SetEmotion(string emotion) {}
        protected override void SetMotorOn(bool on) {}
        public override void ChangeGrip(float left, float right) {}
        public override List<float> GetLatestJointValues()
        {
            return new List<float>();
        }
    }
}