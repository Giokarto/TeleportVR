using System;
using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// This class is gathered and modified from https://github.com/gtk2k/Unity_aiortc_sample
    /// It is responsible with creating the connection with aiortc server and set necessary stream services, such as Data, Video and Audio channels
    /// </summary>
    public class AiortcConnector : MonoBehaviour, IAiortcWebSocket
    {
        public string aiortcServerURL;
        [SerializeField] private RawImage dummyImage;
        [SerializeField] private ImtpEncoder imtpEncoder;
        [SerializeField] private AudioSource receiveAudio;
        [SerializeField] private HeadPositionProtocol _headPositionProtocol;

        public AiortcWebSocket socket;
        private Renderer leftRenderer;
        private Renderer rightRenderer;
        GameObject LeftEye, RightEye;
        private float timeElapsed;
        public float publishMessageFrequency = 5f;

        /// <summary>
        /// Indicates if a data channel is open.
        /// </summary>
        public bool isConnected { get; private set; }


        public RTCPeerConnection pc;
        public RTCDataChannel pingDataChannel, mcDataChannel, jsDataChannel;

        /// <summary>
        /// Initializes game components and data channel behavior then tries to start the connection
        /// </summary>
        void Start()
        {
            socket = new AiortcWebSocket(this);
            StartCoroutine(TryConnect());
            LeftEye = imtpEncoder.leftEye;
            RightEye = imtpEncoder.rightEye;
            leftRenderer = LeftEye.GetComponentInChildren<Renderer>();
            rightRenderer = RightEye.GetComponentInChildren<Renderer>();
            
            WebRTC.Initialize();
            StartCoroutine(WebRTC.Update());
            //Connect();
        }

        private void InitMultimediaTracks(RTCPeerConnection pc)
        {
            pc.OnTrack = e =>
            {
                if (e.Track is VideoStreamTrack video)
                {
                    video.OnVideoReceived += tex =>
                    {
                        if (tex == null)
                        {
                            Debug.Log("dannyb tex is null");
                        }
                        Debug.Log("dannyb OnVideoReceived");
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
        }

        private RTCDataChannel InitPingChannel(RTCPeerConnection pc)
        {
            RTCDataChannelInit conf = new RTCDataChannelInit();
            conf.ordered = true;
            pingDataChannel = pc.CreateDataChannel("ping", conf);
            
            pingDataChannel.OnOpen = () =>
            {
                SendPingMsg();
            };
            pingDataChannel.OnMessage = bytes =>
            {
                Debug.Log("Received message in ping channel!");
                SendPingMsg();
            };
            
            return pingDataChannel;
        }

        private RTCDataChannel InitMotionCompensationChannel(RTCPeerConnection pc)
        {
            var conf = new RTCDataChannelInit();
            conf.ordered = true;
            
            mcDataChannel = pc.CreateDataChannel("motion_compensation", conf);
            mcDataChannel.OnOpen = () =>
            {
                Debug.Log("New channel open: movement compensation data channel");
            };
            
            return mcDataChannel;
        }
        
        private RTCDataChannel InitJointStatesChannel(RTCPeerConnection pc)
        {
            var conf = new RTCDataChannelInit();
            conf.ordered = true;
            
            jsDataChannel = pc.CreateDataChannel("joint_targets", conf);
            jsDataChannel.OnOpen = () =>
            {
                Debug.Log("New channel open: joint state data channel");
            };

            return jsDataChannel;
        }
        
        /// <summary>
        /// Sends message to data channel
        /// </summary>
        private void SendPingMsg()
        {
            pingDataChannel.Send("ping");
        }
        
        /// <summary>
        /// Stops the webrtc connection
        /// </summary>
        public void Stop()
        {
            /*
            GameObject obj = GameObject.Find("AiortcWebSocket");
            AiortcWebSocket comp = obj.GetComponent<AiortcWebSocket>();
            if (comp != null) {
                Destroy(comp);
            }*/
            
            receiveAudio.Stop();
            receiveAudio.clip = null;
            WebRTC.Dispose();
        }

        private RTCIceServer[] GetICEServers(string[] urls)
        {
            Debug.Log("DannyB stun servers:");
            List<RTCIceServer> servers = new List<RTCIceServer>();
            foreach (var url in urls)
            {
                if (url.StartsWith("turn"))
                {
                    servers.Add(new RTCIceServer
                    {
                        urls = new string[] { url },
                        username = "user",
                        credential = "password"
                    });
                }
                else
                {
                    servers.Add(new RTCIceServer
                    {
                        urls = new string[] { url },
                    });
                }
            }
            
            return servers.ToArray();
        }
        
        /// <summary>
        /// Sets webrtc connection behaviors by setting up delegate functions, onTrack is very important in particular where incoming connections are listened
        /// Then makes a offer for connection
        /// </summary>
        public void Connect(string[] urls)
        {
            
            var c = new RTCConfiguration();
            c.iceServers = GetICEServers(urls);
            pc = new RTCPeerConnection(ref c);
            pc.OnIceCandidate = cand =>
            {
                pc.OnIceCandidate = null;

                if (socket.robot_name != null)
                {
                    Debug.Log("dannyB offering connection!");
                    // TODO: "OnIceCandidate" is a wrong place for this
                    socket.OfferConnection();
                }
                else
                {
                    Debug.Log("dannyB no robot to operate!");
                }
            };
            pc.OnConnectionStateChange = state =>
            {
                Debug.Log("dannyb OnConnectionStateChange " + state);
                switch (state)
                {
                    case RTCPeerConnectionState.Connected:
                        isConnected = true;
                        break;
                    default:
                        isConnected = false;
                        break;
                }
            };
            InitMultimediaTracks(pc);
            
            pingDataChannel = InitPingChannel(pc);
            mcDataChannel = InitMotionCompensationChannel(pc);
            jsDataChannel = InitJointStatesChannel(pc);
            
            
            StartCoroutine(CreateDesc(RTCSdpType.Offer));
        }

        
        public enum Side
        {
            Local,
            Remote
        }

        /// <summary>
        /// Creates description of connection
        /// </summary>
        private IEnumerator CreateDesc(RTCSdpType type)
        {
            Debug.Log("dannyb CreateDesc");
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

        public IEnumerator SetDesc(Side side, RTCSessionDescription desc)
        {
            Debug.Log("dannyB setdesc for side " + side + ":" + desc.sdp);
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

        public IEnumerator TryConnect()
        {
            while (socket.stun_urls == null)
            {
                Debug.Log("DannyB No stuns yet");
                yield return new WaitForSeconds(2);
            }
            try
            {
                Connect(socket.stun_urls);
            }
            catch (Exception e)
            {
                Debug.LogError("Connection not successful!");
                Debug.LogException(e);
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
        

        /*
        [SerializeField] private VideoTransformType videoTransformType;
        
        public enum VideoTransformType
        {
            None,
            EdgeDetection,
            CartoonEffect,
            Rotate
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
        }*/

        /// <summary>
        /// Webrtc frame updates are listened here and for every incoming texture it is setting spherical game object texture with coming frames
        /// Frames can be also transfered to ImtpEncoder class and can be manipulated further, currently disabled because it is causing rendering 
        /// problems on Oculus 
        /// </summary>
        void Update()
        {
            leftRenderer.material.mainTexture = dummyImage.texture as Texture2D;
            rightRenderer.material.mainTexture = dummyImage.texture as Texture2D;

            timeElapsed += Time.deltaTime;

            if (timeElapsed > publishMessageFrequency && isConnected)
            {
                SendPingMsg();
                timeElapsed = 0;
            }
        }
        public void OnWebRtcConfigGathered(string[] urls)
        {
        }

        public void OnRobotConnected(string robotName)
        {
            
        }

        public RTCPeerConnection GetPeerConnection()
        {
            return pc;
        }

        public void SetRemoteDescription(ref RTCSessionDescription rtcSessionDescription)
        {
            pc.SetRemoteDescription(ref rtcSessionDescription);
        }
    }
}
