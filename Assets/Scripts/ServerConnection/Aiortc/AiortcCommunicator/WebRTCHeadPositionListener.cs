using System;
using System.Text;
using Newtonsoft.Json;
using ServerConnection.Aiortc;
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
            dataChannel = GetComponent<AiortcConnector>().pingDataChannel;
            dataChannel.OnMessage += OnMessage;
        }

        private void OnMessage(byte[] bytes)
        {
            var str = System.Text.Encoding.UTF8.GetString(bytes);
            try
            {
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

        public class HeadPositionMessage
        {
            public float head_axis0 { get; set; }
            public float head_axis1 { get; set; }
            public float head_axis2 { get; set; }

            public Vector3 toVector3()
            {
                return new Vector3(head_axis0, head_axis1, head_axis2);
            }
        }
    }
}