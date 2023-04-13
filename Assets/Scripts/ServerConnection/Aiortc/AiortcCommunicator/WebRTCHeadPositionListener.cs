using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerConnection.Aiortc;
using ServerConnection.ServerCommunicatorBase;
using UnityEngine;
using UnityEngine.XR;
using Unity.WebRTC;
using Random = System.Random;


namespace ServerConnection.Aiortc
{
    /// <summary>
    /// This class handles of initialization and handle event of head position channel
    /// </summary>
    public class WebRTCHeadPositionListener : HeadPositionListenerBase
    {
        private RTCPeerConnection peerConnection;
        public RTCDataChannel dataChannel;
        private bool goingUp = true;
        private float current = -0.5f;

        public void Start()
        {
            base.Start();
            Debug.Log("initializing WebRTCHeadPositionListener");
        }

        /// <summary>
        /// handles incoming head position data
        /// </summary>
        public void OnMessage(byte[] bytes)
        {
            var str = System.Text.Encoding.UTF8.GetString(bytes);
            try
            {
                str.Remove(0, 5);
                JObject jsonObject = JObject.Parse(str);
                JToken myToken = jsonObject["head position"];
                //var name = myToken.ToObject<HeadPositionMessage>();
                var obj = JsonConvert.DeserializeObject<HeadPositionMessage>(Encoding.UTF8.GetString(bytes));
                ProcessHeadMessage(obj.toVector3());
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Dispose()
        {
            if (dataChannel != null)
            {
                dataChannel.OnMessage -= OnMessage;
            }
        }
    }
}