using System;
using System.Text;
using Newtonsoft.Json;
using ServerConnection.Aiortc;
using UnityEngine;
using UnityEngine.XR;
using Unity.WebRTC;
using Random = System.Random;

public class WebRTCHeadPositionListener : HeadPositionListenerBase
{
    private RTCPeerConnection peerConnection;
    private RTCDataChannel dataChannel;

    public void Start()
    {
        base.Start();
        Debug.Log("dannyb initializing WebRTCHeadPositionListener");
        dataChannel = GetComponent<AiortcConnector>().dataChannel;
        dataChannel.OnMessage += OnMessage;
    }

    private void OnMessage(byte[] bytes)
    {
        var str = System.Text.Encoding.UTF8.GetString(bytes);
        //Debug.Log("DannyB: " + str);
        // Create a new instance of the Random class
        var random = new Random();

        // Generate three random radians between 0 and 2*pi (i.e., a full circle)
        float randomRadians1 = (float)(random.NextDouble() * Math.PI * 2.0);
        //var obj = JsonConvert.DeserializeObject<HeadPositionMessage>(Encoding.UTF8.GetString(bytes));
        //if (obj != null)
        ProcessHeadMessage(new Vector3(0f,randomRadians1,0f));
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
