using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;

public class aiortcConnector : MonoBehaviour
{
    [SerializeField] private string aiortcServerURL;
    [SerializeField] private RawImage dummyImage;
    [SerializeField] private VideoTransformType videoTransformType;
    [SerializeField] private ImtpEncoder imtpEncoder;
    [SerializeField] private AudioSource receiveAudio;

    public enum VideoTransformType
    {
        None,
        EdgeDetection,
        CartoonEffect,
        Rotate
    }

    private enum Side
    {
        Local,
        Remote
    }

    private class SignalingMsg
    {
        public string type;
        public string sdp;
        public string video_transform;
        public RTCSessionDescription ToDesc()
        {
            return new RTCSessionDescription
            {
                type = type == "offer" ? RTCSdpType.Offer : RTCSdpType.Answer,
                sdp = sdp
            };
        }
    }

    private RTCPeerConnection pc;
    private MediaStream receiveStream;
    
    private RTCDataChannel dataChannel, remoteDataChannel;
    private DelegateOnMessage onDataChannelMessage;
    private DelegateOnOpen onDataChannelOpen;
    private DelegateOnClose onDataChannelClose;
    private DelegateOnDataChannel onDataChannel;

    public Int32[][] faceCoordinates = {};

    void Start()
    {
        
        onDataChannel = channel =>
        {
            Debug.Log($"onDataChannel");
            remoteDataChannel = channel;
            remoteDataChannel.OnMessage = onDataChannelMessage;
        };
        onDataChannelMessage = bytes =>
        {
            var str = System.Text.Encoding.UTF8.GetString(bytes);
            if (str.StartsWith("faces: "))
            {
                faceCoordinates = JsonConvert.DeserializeObject<Int32[][]>(str.Substring(7));

                var texture2D = dummyImage.texture as Texture2D;
                foreach (var face in faceCoordinates)
                {
                    Debug.Log($"red face {face[0]}, {face[1]}, {face[2]}, {face[3]}");
                    for (int i = face[0]; i < face[0]+face[2]; i++)
                    {
                        for (int j = face[1]; j < face[1]+face[3]; j++)
                        {
                            texture2D.SetPixel(i, texture2D.height - j, Color.red);
                        }
                    }
                }
                texture2D.Apply();
                dummyImage.texture = texture2D;
            }
            //Debug.Log($"{str}");
            //textReceive.text = System.Text.Encoding.UTF8.GetString(bytes);
        };
        onDataChannelOpen = () =>
        {
            Debug.Log($"onDataChannelOpen");
            SendMsg();
        };
        onDataChannelClose = () =>
        {
            Debug.Log($"onDataChannelClose");
        };
        WebRTC.Initialize();
        StartCoroutine(WebRTC.Update());
        Connect();
    }

    public void Stop()
    {
        receiveAudio.Stop();
        receiveAudio.clip = null;
        WebRTC.Dispose();
    }
    
    public void Connect()
    {
        receiveStream = new MediaStream();
        receiveStream.OnAddTrack = e =>
        {
            if (e.Track is VideoStreamTrack track)
            {
                // You can access received texture using `track.Texture` property.
                Debug.Log("receiveStream.OnAddTrack ");
            }
            else if (e.Track is AudioStreamTrack track2)
            {
                // This track is for audio.
                
            }
        };
        receiveStream.OnRemoveTrack = ev =>
        {
            Debug.Log("receiveStream.OnRemoveTrack");
            dummyImage.texture = null;
            ev.Track.Dispose();
        };

        pc = new RTCPeerConnection();
        pc.OnIceCandidate = cand =>
        {
            pc.OnIceCandidate = null;
            var msg = new SignalingMsg
            {
                type = pc.LocalDescription.type.ToString().ToLower(),
                sdp = pc.LocalDescription.sdp
            };

            switch (videoTransformType)
            {
                case VideoTransformType.None: msg.video_transform = "none"; break;
                case VideoTransformType.EdgeDetection: msg.video_transform = "edges"; break;
                case VideoTransformType.CartoonEffect: msg.video_transform = "cartoon"; break;
                case VideoTransformType.Rotate: msg.video_transform = "rotate"; break;
            }

            StartCoroutine(aiortcSignaling(msg));
        };
        pc.OnIceGatheringStateChange = state =>
        {
            Debug.Log($"OnIceGatheringStateChange > state: {state}");
        };
        pc.OnConnectionStateChange = state =>
        {
            Debug.Log($"OnConnectionStateChange > state: {state}");
        };
        pc.OnTrack = e =>
        {
            Debug.Log($"OnTrack");
            if (e.Track.Kind == TrackKind.Video)
            {
                // Add track to MediaStream for receiver.
                // This process triggers `OnAddTrack` event of `MediaStream`.
                receiveStream.AddTrack(e.Track);
            }
            if (e.Track is VideoStreamTrack video)
            {
                Debug.Log($"OnTrackVideo");

                video.OnVideoReceived += tex =>
                {
                    Debug.Log("onvideoreceived");
                    dummyImage.texture = tex;
                };
            }
            if (e.Track is AudioStreamTrack audioTrack)
            {
                receiveAudio.SetTrack(audioTrack);
                receiveAudio.loop = true;
                receiveAudio.Play();
            }
        };
        pc.OnDataChannel = onDataChannel;
        RTCDataChannelInit conf = new RTCDataChannelInit();
        conf.ordered = true;
        dataChannel = pc.CreateDataChannel("ping", conf);
        dataChannel.OnOpen = onDataChannelOpen;
        dataChannel.OnMessage = onDataChannelMessage;
        StartCoroutine(CreateDesc(RTCSdpType.Offer));
    }

    private IEnumerator CreateDesc(RTCSdpType type){
        if(type == RTCSdpType.Offer)
        {
            var transceiver1 = pc.AddTransceiver(TrackKind.Video);
            transceiver1.Direction = RTCRtpTransceiverDirection.RecvOnly;
            var transceiver2 = pc.AddTransceiver(TrackKind.Audio);
            transceiver2.Direction = RTCRtpTransceiverDirection.RecvOnly;
            Debug.Log("Offer");
        }
        var op = type == RTCSdpType.Offer ? pc.CreateOffer() : pc.CreateAnswer();
        yield return op;

        if (op.IsError)
        {
            Debug.LogError($"Create {type} Error: {op.Error.message}");
            yield break;
        }

        StartCoroutine(SetDesc(Side.Local, op.Desc));
    }

    private IEnumerator SetDesc(Side side, RTCSessionDescription desc)
    {
        var op = side == Side.Local ? pc.SetLocalDescription(ref desc) : pc.SetRemoteDescription(ref desc);
        yield return op;

        if (op.IsError)
        {
            Debug.Log($"Set {desc.type} Error: {op.Error.message}");
            yield break;
        }

        if (side == Side.Local)
        {
            // aiortc not support Tricle ICE. 
        }
        else if (desc.type == RTCSdpType.Offer)
        {
            StartCoroutine(CreateDesc(RTCSdpType.Answer));
        }
    }

    private IEnumerator aiortcSignaling(SignalingMsg msg)
    {
        var jsonStr = JsonUtility.ToJson(msg);
        using var req = new UnityWebRequest($"{aiortcServerURL}/{msg.type}", "POST");
        var bodyRaw = Encoding.UTF8.GetBytes(jsonStr);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
        
        Debug.Log($"aiortcSignaling1: {msg}");
        Debug.Log($"aiortcSignaling2: {jsonStr}");
        Debug.Log($"aiortcSignaling3: {bodyRaw}");
        Debug.Log($"aiortcSignaling4: {req.url}");
        Debug.Log($"aiortcSignaling5: {req.downloadHandler.text}");
        
        var resMsg = JsonUtility.FromJson<SignalingMsg>(req.downloadHandler.text);

        StartCoroutine(SetDesc(Side.Remote, resMsg.ToDesc()));
    }

    void Update()
    {
        if (imtpEncoder != null)
        {
            imtpEncoder.SetLastReceivedTexture(dummyImage.texture);
        }
        /*
        var originalTargetTexture = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = originalTargetTexture;*/
    }
    public void SendMsg()
    {
        Debug.Log($"SendMsg ping");
        dataChannel.Send("ping");
    }
}

