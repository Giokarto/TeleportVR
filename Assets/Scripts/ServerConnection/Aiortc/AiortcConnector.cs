using System;
using System.Collections;
using System.Text;
using System.Threading;
using CurvedUI;
using Newtonsoft.Json;
using ServerConnection.RosTcpConnector;
using Unity.WebRTC;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// This class is gathered and modified from https://github.com/gtk2k/Unity_aiortc_sample
    /// It is responsible with creating the connection with aiortc server and set necessary stream services, such as Data, Video and Audio channels
    /// </summary>
    public class AiortcConnector : MonoBehaviour
    {
        public string aiortcServerURL;
        [SerializeField] private RawImage dummyImage;
        [SerializeField] private VideoTransformType videoTransformType;
        [SerializeField] private ImtpEncoder imtpEncoder;
        [SerializeField] private AudioSource receiveAudio;
        [SerializeField] private HeadPositionProtocol _headPositionProtocol;

        private Renderer leftRenderer;
        private Renderer rightRenderer;
        GameObject LeftEye, RightEye;
        private bool initialized = false;
        private Renderer leftFaceDetectionRenderer;
        private float timeElapsed;
        public float publishMessageFrequency = 1.0f;

        /// <summary>
        /// Indicates if a data channel is open.
        /// </summary>
        public bool isConnected { get; private set; }

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


        /// <summary>
        /// Sets the signalling message which can be offer for outgoing connection and answer for incomming connection
        /// </summary>
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

        public RTCDataChannel pingDataChannel, mcDataChannel, jsDataChannel;
        private DelegateOnMessage onDataChannelMessage;
        private DelegateOnOpen onDataChannelOpen, onMCDataChannelOpen, onJSDataChannelOpen;
        private DelegateOnClose onDataChannelClose;
        private DelegateOnDataChannel onDataChannel;

        /// <summary>
        /// Initializes game components and data channel behavior then tries to start the connection
        /// </summary>
        void Start()
        {
            LeftEye = imtpEncoder.leftEye;
            leftRenderer = imtpEncoder.leftEye.GetNamedChild("LeftSphereMaterial").GetComponent<Renderer>();
            leftFaceDetectionRenderer = imtpEncoder.leftEye.GetNamedChild("Plane").GetComponent<Renderer>();
            leftFaceDetectionRenderer.material.mainTexture = new Texture2D(1080, 1080);
            rightRenderer = imtpEncoder.rightEye.GetComponentInChildren<Renderer>();
            RightEye = imtpEncoder.rightEye;
            onDataChannel = channel =>
            {
                Debug.Log("dannyb current channel name: " + channel.Label);
            };
            onDataChannelMessage = bytes =>
            {
                var str = System.Text.Encoding.UTF8.GetString(bytes);
                if (str.StartsWith("faces: "))
                {
                    imtpEncoder.faceCoordinates = JsonConvert.DeserializeObject<Int32[][]>(str.Substring(7));
                }

                if (!initialized)
                {
                    this.AddComponentIfMissing <WebRTCHeadPositionListener>();
                    this.AddComponentIfMissing <WebRTCJointPositionSender>();
                    initialized = true;
                }

                Debug.Log(str);
            };
            onDataChannelOpen = () =>
            {
                SendMsg();
                isConnected = true;
            };
            onDataChannelClose = () =>
            {
                isConnected = false;
            };

            onMCDataChannelOpen = () =>
            {
                Debug.Log("dannyb opens movement compensation data channel");
            };
            
            onJSDataChannelOpen = () =>
            {
                Debug.Log("dannyb opens joint state  data channel");
            };
            WebRTC.Initialize();
            StartCoroutine(WebRTC.Update());
            Connect();
        }

        /// <summary>
        /// Stops the webrtc connection
        /// </summary>
        public void Stop()
        {
            receiveAudio.Stop();
            receiveAudio.clip = null;
            WebRTC.Dispose();
        }

        /// <summary>
        /// Sets webrtc connection behaviors by setting up delegate functions, onTrack is very important in particular where incoming connections are listened
        /// Then makes a offer for connection
        /// </summary>
        public void Connect()
        {
            receiveStream = new MediaStream();
            receiveStream.OnAddTrack = e =>
            {
                if (e.Track is VideoStreamTrack track)
                {
                    // You can access received texture using `track.Texture` property.
                }
                else if (e.Track is AudioStreamTrack track2)
                {
                    // This track is for audio.

                }
            };
            receiveStream.OnRemoveTrack = ev =>
            {
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
                    case VideoTransformType.None:
                        msg.video_transform = "none";
                        break;
                    case VideoTransformType.EdgeDetection:
                        msg.video_transform = "edges";
                        break;
                    case VideoTransformType.CartoonEffect:
                        msg.video_transform = "cartoon";
                        break;
                    case VideoTransformType.Rotate:
                        msg.video_transform = "rotate";
                        break;
                }

                StartCoroutine(aiortcSignaling(msg));
            };
            pc.OnIceGatheringStateChange = state => { };
            pc.OnConnectionStateChange = state => { };
            pc.OnTrack = e =>
            {
                if (e.Track.Kind == TrackKind.Video)
                {
                    // Add track to MediaStream for receiver.
                    // This process triggers `OnAddTrack` event of `MediaStream`.
                    receiveStream.AddTrack(e.Track);
                }

                if (e.Track is VideoStreamTrack video)
                {

                    video.OnVideoReceived += tex => { dummyImage.texture = tex; };
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
            pingDataChannel = pc.CreateDataChannel("ping", conf);
            pingDataChannel.OnOpen = onDataChannelOpen;
            pingDataChannel.OnMessage = onDataChannelMessage;
            mcDataChannel = pc.CreateDataChannel("motion_compensation", conf);
            mcDataChannel.OnOpen = onMCDataChannelOpen;
            jsDataChannel = pc.CreateDataChannel("joint_state", conf);
            jsDataChannel.OnOpen = onJSDataChannelOpen;
            StartCoroutine(CreateDesc(RTCSdpType.Offer));
        }

        /// <summary>
        /// Creates description of connection
        /// </summary>
        private IEnumerator CreateDesc(RTCSdpType type)
        {
            if (type == RTCSdpType.Offer)
            {
                var transceiver1 = pc.AddTransceiver(TrackKind.Video);
                transceiver1.Direction = RTCRtpTransceiverDirection.RecvOnly;
                var transceiver2 = pc.AddTransceiver(TrackKind.Audio);
                transceiver2.Direction = RTCRtpTransceiverDirection.RecvOnly;
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

        /// <summary>
        /// GetDataChannel
        /// </summary>
        public RTCDataChannel GetDataChannel(string name)
        {
            if (pingDataChannel.Label == name)
            {
                return pingDataChannel;
            }
            else
            {
                Debug.LogWarning("Data channel not found: " + name);
                return null;
            }
        }
        
        /// <summary>
        /// Sends connection web request to dedicated server
        /// </summary>
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
            Debug.Log(resMsg);

            StartCoroutine(SetDesc(Side.Remote, resMsg.ToDesc()));
        }

        /// <summary>
        /// Webrtc frame updates are listened here and for every incoming texture it is setting spherical game object texture with coming frames
        /// Frames can be also transfered to ImtpEncoder class and can be manipulated further, currently disabled because it is causing rendering 
        /// problems on Oculus 
        /// </summary>
        void Update()
        {
            // if (imtpEncoder != null)
            // {
            //     imtpEncoder.SetLastReceivedTexture(dummyImage.texture);
            // }

            leftRenderer.material.mainTexture = dummyImage.texture as Texture2D;
            rightRenderer.material.mainTexture = dummyImage.texture as Texture2D;

            timeElapsed += Time.deltaTime;

            if (timeElapsed > publishMessageFrequency && initialized)
            {
                SendMsg();
                timeElapsed = 0;
            }
            /*
            var originalTargetTexture = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = originalTargetTexture;*/
        }

        /// <summary>
        /// Sends message to data channel
        /// </summary>
        public void SendMsg()
        {
            Debug.Log($"SendMsg ping");
            pingDataChannel.Send("ping");
        }
    }
}
