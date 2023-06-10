using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using RobodyControl;
using UnityEngine;
using UnityEngine.Video;
using Random = System.Random;

namespace ServerConnection.MockServer
{
    /// <summary>
    /// This class mocks a server connection. It displays an Obama video instead of real-world stream.
    /// </summary>
    public class MockServer : ServerBase
    {
        private Random random = new Random();

        private VideoPlayer videoPlayer;
        private int fps = 10;

        
        private Texture2D _leftTexture;
        private Texture2D _rightTexture;
        private Renderer _leftRenderer;
        private Renderer _rightRenderer;
        private Texture2D _leftReveresTexture;
        private Texture2D _rightReveresTexture;
        private Renderer _leftReveresRenderer;
        private Renderer _rightReverseRenderer;

        private bool _connectedToServer;

        public override string IPaddress
        {
            get => "localhost";
            set {}
        }
        
        public void Start()
        {
            _connectedToServer = false;
            
            videoPlayer = new GameObject("mockVideo", typeof(VideoPlayer)).GetComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.url = "C:\\Users\\Roboy\\projects\\src\\github.com\\Roboy\\ObamaVideoTest.mp4";
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
            
            _leftRenderer = LeftEye.GetComponentInChildren<Renderer>();
            _rightRenderer = RightEye.GetComponentInChildren<Renderer>();
            
            
            _leftReveresRenderer = LeftReverseEye.GetComponentInChildren<Renderer>();
            _rightReverseRenderer = RightReverseEye.GetComponentInChildren<Renderer>();
        }
        
        /// <summary>
        /// This is especially inefficient with creating a new Texture in every update. Use only for testing!
        /// </summary>
        public void Update()
        {
            _connectedToServer = random.NextDouble() < 0.98 ? ConnectedToServer : !ConnectedToServer;
            foreach (Modality modality in Enum.GetValues(typeof(Modality)))
            {
                var change = random.NextDouble() >= 0.97;
                ModalityConnected[modality] = !change
                    ? ModalityConnected[modality]
                    : !ModalityConnected[modality];
            }

            Texture2D tex = new Texture2D(360, 360);
            byte[] test = File.ReadAllBytes("C:\\Users\\Roboy\\projects\\src\\github.com\\Roboy\\image_double.jpg");
            tex.LoadImage(test);
            
            _leftRenderer.material.mainTexture = tex;
            _rightRenderer.material.mainTexture = tex;
            _leftReveresRenderer.material.mainTexture = tex;
           _rightReverseRenderer.material.mainTexture = tex;
        }

        public override bool ConnectedToServer => _connectedToServer;

        public override Dictionary<Modality, bool> ModalityConnected { get; }
            = Enum.GetValues(typeof(Modality)).Cast<Enum>().ToDictionary(e => (Modality)e, v => false);

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
            throw new NotImplementedException();
        }
    }
}