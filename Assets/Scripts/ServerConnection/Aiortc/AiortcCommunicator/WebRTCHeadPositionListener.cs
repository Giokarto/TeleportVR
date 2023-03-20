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
    public class WebRTCHeadPositionListener : HeadPositionListenerBase
    {
        private RTCPeerConnection peerConnection;
        private RTCDataChannel dataChannel;
        private bool goingUp = true;
        private float current = -0.5f;

        public void Start()
        {
            base.Start();
            Debug.Log("dannyb initializing WebRTCHeadPositionListener");
        }

        public void Update()
        {
            if (dataChannel == null)
            {
                dataChannel = FindObjectOfType<AiortcConnector>().pingDataChannel;
                dataChannel.OnMessage += OnMessage;
            }
        }

        //dannyb OnMessage: stats {"Codec": "H264Encoder / libx264", " Target FPS": "5", "Current FPS": "5.00500464188291", "Target Resolution": "480p", "Est. Bandwidth": 246.88, "...Target kBit": 10000.0, "fpsTarget kBit": 1000.0, "..Current kBit": 141.92476950792314, "head position": "{\"head_axis0\": -8.916703717249375e-29, \"head_axis1\": 4.956035581926682e-13, \"head_axis2\": 9.499068198692826e-12}"}
        private void OnMessage(byte[] bytes)
        {
            var str = System.Text.Encoding.UTF8.GetString(bytes);
            try
            {
                str.Remove(0, 5);
                Debug.Log("Dannyb XXXX" + str);
                JObject jsonObject = JObject.Parse(str);
                JToken myToken = jsonObject["head position"];
                //var name = myToken.ToObject<HeadPositionMessage>();
                Debug.Log("dannyb head name: " + myToken);
                var obj = new HeadPositionMessage();//JsonConvert.DeserializeObject<HeadPositionMessage>(Encoding.UTF8.GetString(bytes));
                if (obj != null)
                {
                    if (current <= -0.5f)
                    {
                        goingUp = true;
                    }

                    if (current >= 0.5f)
                    {
                        goingUp = false;
                    }

                    if (goingUp)
                    {
                        current += 0.001f;
                    }
                    else
                    {
                        current -= 0.001f;
                    }

                    obj.head_axis0 = current; //up down
                    obj.head_axis1 = 0f;//right left
                    obj.head_axis2 = 0f;//yaw
                    ProcessHeadMessage(obj.toVector3());
                }
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