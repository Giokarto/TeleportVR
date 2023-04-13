using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;
using Widgets;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// This class is gathered and modified from https://github.com/gtk2k/Unity_aiortc_sample
    /// It is responsible with creating the connection with aiortc server and set necessary stream services, such as Data, Video and Audio channels
    /// </summary>
    public class AiortcConnector : MonoBehaviour, IAiortcWebSocket
    {
        public string aiortcServerURL;
        [SerializeField] public RawImage dummyImage;
        [SerializeField] private bool UsingSocket;
        [SerializeField] private AudioSource receiveAudio;

        private bool currentlyStreaming = false;


        public AiortcWebSocket socket;
        private float timeElapsed;
        public float publishMessageFrequency = 5f;

        private bool peerConnectionConnected;
        public bool RobotConnected { get; private set; }


        public RTCPeerConnection pc;
        public RTCDataChannel pingDataChannel, mcDataChannel, jsDataChannel, wcDataChannel;

        public WebRTCHeadPositionListener headPositionListener;
        public WebRTCJointPositionSender jointPositionSender;
        public WebRTCWheelCommandSender wheelCommandSender;


        private VideoStreamTrack ReceiveVideo;
        /// <summary>
        /// Initializes game components and data channel behavior then tries to start the connection
        /// </summary>
        void Start()
        {
            WebRTC.Initialize();
            StartCoroutine(WebRTC.Update());
            if (!UsingSocket)
            {
                Connect(null);
            }
            else
            {
                socket = new AiortcWebSocket(this);
                StartCoroutine(TryConnectWithSocket());
            }
        }

        /// <summary>
        /// Enable / disable sending and rendering received data for all modalities.
        /// </summary>
        public void SetMotorOn(bool on)
        {
            currentlyStreaming = on;
            headPositionListener.enabled = on;
            jointPositionSender.enabled = on;
            wheelCommandSender.enabled = on;
            if (videoTransceiver != null)
            {
                videoTransceiver.Receiver.Track.Enabled = true; // on;
            }

            if (audioTransciever != null)
            {
                audioTransciever.Sender.Track.Enabled = on;
                audioTransciever.Receiver.Track.Enabled = on;
            }
            receiveAudio.enabled = on;
        }


        private string _defaulMicrophone;
        private AudioClip m_clipInput;
        private MediaStream _sendStream;
        [SerializeField] private AudioSource _audioSourceInput;
        private AudioStreamTrack micInputTrack;
        private RTCRtpTransceiver videoTransceiver;
        private RTCRtpTransceiver audioTransciever;
        
        /// <summary>
        /// Initializes multimedia tracks. adds operator audio channel as well as on track listeners
        /// </summary>
        private void InitMultimediaTracks(RTCPeerConnection pc)
        {
            pc.OnTrack = e =>
            {
                if (e.Track is VideoStreamTrack video)
                {
                    Debug.Log("other side added video stream track");
                    ReceiveVideo = video;
                    ReceiveVideo.OnVideoReceived += tex =>
                    {
                        if (tex == null)
                        {
                            Debug.Log("tex is null");
                        }

                        Debug.Log("OnVideoReceived");
                        dummyImage.texture = tex;
                    };
                }

                if (e.Track is AudioStreamTrack audioTrack)
                {
                    Debug.Log("audio source added");
                    receiveAudio.SetTrack(audioTrack);
                    receiveAudio.loop = true;
                    receiveAudio.Play();
                    receiveAudio.enabled = currentlyStreaming;
                }
            };
            
            videoTransceiver = pc.AddTransceiver(TrackKind.Video);
            videoTransceiver.Direction = RTCRtpTransceiverDirection.RecvOnly;
            audioTransciever = pc.AddTransceiver(TrackKind.Audio);
            audioTransciever.Direction = RTCRtpTransceiverDirection.SendRecv;



            //initialize outgoing audio properties
            _defaulMicrophone = Microphone.devices[0];
            Microphone.GetDeviceCaps(_defaulMicrophone, out int minFreq, out int maxFreq);
            m_clipInput = Microphone.Start(_defaulMicrophone, true, 1, 48000);

            _sendStream = new MediaStream();
            _audioSourceInput.clip = m_clipInput;
            _audioSourceInput.loop = true;
            _audioSourceInput.Play();
            micInputTrack = new AudioStreamTrack(_audioSourceInput);
            audioTransciever.Sender.ReplaceTrack(micInputTrack);


            videoTransceiver.Receiver.Track.Enabled = true; // currentlyStreaming;
            audioTransciever.Sender.Track.Enabled = currentlyStreaming;
            audioTransciever.Receiver.Track.Enabled = currentlyStreaming;

        }

        /// <summary>
        /// Initializes ping data channel
        /// </summary>
        private RTCDataChannel InitPingChannel(RTCPeerConnection pc)
        {
            RTCDataChannelInit conf = new RTCDataChannelInit();
            conf.ordered = true;
            pingDataChannel = pc.CreateDataChannel("ping", conf);

            pingDataChannel.OnOpen = () => { SendPingMsg(); };
            pingDataChannel.OnMessage = bytes =>
            {
                //Debug.Log("Received message in ping channel!");
            };

            pingDataChannel.OnClose = () => { Debug.Log("Ping channel is closed!"); };

            return pingDataChannel;
        }
        
        /// <summary>
        /// Initializes motion compensation data channel
        /// </summary>
        private RTCDataChannel InitMotionCompensationChannel(RTCPeerConnection pc)
        {
            var conf = new RTCDataChannelInit();
            conf.ordered = true;

            mcDataChannel = pc.CreateDataChannel("motion_compensation", conf);
            mcDataChannel.OnOpen = () => { Debug.Log("New channel open: movement compensation data channel"); };

            headPositionListener.dataChannel = mcDataChannel;
            mcDataChannel.OnMessage += headPositionListener.OnMessage;

            return mcDataChannel;
        }

        /// <summary>
        /// Initializes joint states data channel
        /// </summary>
        private RTCDataChannel InitJointStatesChannel(RTCPeerConnection pc)
        {
            var conf = new RTCDataChannelInit();
            conf.ordered = true;

            jsDataChannel = pc.CreateDataChannel("joint_targets", conf);
            jsDataChannel.OnOpen = () => { Debug.Log("New channel open: joint state data channel"); };

            jointPositionSender.dataChannel = jsDataChannel;

            return jsDataChannel;
        }

        /// <summary>
        /// Initializes joint states data channel
        /// </summary>
        private RTCDataChannel InitWheelCommandsChannel(RTCPeerConnection pc)
        {
            var conf = new RTCDataChannelInit();
            conf.ordered = true;

            wcDataChannel = pc.CreateDataChannel("wheel_commands", conf);
            wcDataChannel.OnOpen = () => { Debug.Log("New channel open: wheel commands data channel"); };

            wheelCommandSender.dataChannel = wcDataChannel;

            return wcDataChannel;
        }

        /// <summary>
        /// Sends message to data channel
        /// </summary>
        private void SendPingMsg()
        {
            if (pingDataChannel.ReadyState == RTCDataChannelState.Open)
            {
                pingDataChannel.Send("ping");
            }
            else
            {
                Debug.Log("ping data channel not open");
            }
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
            Debug.Log("stun servers:");
            List<RTCIceServer> servers = new List<RTCIceServer>();
            
            servers.Add(new RTCIceServer
            {
                urls = new string[] { "turn:83.229.87.110:3478" },
                username = "roboy",
                credential = "4dE5?3sgPb0fOrw5Vh"
            });
            servers.Add(new RTCIceServer
            {
                urls = new string[] { "stun:stun.l.google.com:19302" },
            });
            return servers.ToArray();
        }

        private DateTime lastIceCandidateTime;

        /// <summary>
        /// Sets webrtc connection behaviors by setting up delegate functions, onTrack is very important in particular where incoming connections are listened
        /// Then makes a offer for connection
        /// </summary>
        public void Connect(string[] urls)
        {
            if (urls == null)
            {
                pc = new RTCPeerConnection();
            }
            else
            {
                var c = new RTCConfiguration();
                c.iceServers = GetICEServers(urls);
                pc = new RTCPeerConnection(ref c);
            }

            pc.OnIceCandidate = cand =>
            {
                lastIceCandidateTime = DateTime.Now;
                anyIce = true;
            };


            pc.OnIceGatheringStateChange = state =>
            {
                Debug.Log("OnIceGatheringStateChange: " + state);
                if (state == RTCIceGatheringState.Complete)
                {
                    Debug.Log("ICE gathering finished -> starting connection");
                    StartConnection();
                }
            };

            pc.OnConnectionStateChange = state =>
            {
                Debug.Log("OnConnectionStateChange " + state);
                switch (state)
                {
                    case RTCPeerConnectionState.Connected:
                        peerConnectionConnected = true;
                        RobotConnected = true;
                        break;
                    default:
                        peerConnectionConnected = false;
                        RobotConnected = false;
                        break;
                }
            };
            InitMultimediaTracks(pc);

            pingDataChannel = InitPingChannel(pc);
            mcDataChannel = InitMotionCompensationChannel(pc);
            jsDataChannel = InitJointStatesChannel(pc);
            wcDataChannel = InitWheelCommandsChannel(pc);


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
            Debug.Log("CreateDesc");
            if (type == RTCSdpType.Offer)
            {
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
        
        /// <summary>
        /// Sets description of connection according to side
        /// </summary>
        public IEnumerator SetDesc(Side side, RTCSessionDescription desc)
        {
            Debug.Log("setdesc for side " + side + ":" + desc.sdp);
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
        /// When connection is done through socket, whenever stun urls available this method tries to start the connection
        /// </summary>
        public IEnumerator TryConnectWithSocket()
        {
            while (socket.stun_urls == null)
            {
                Debug.Log("No stuns yet");
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

            if (resMsg == null)
            {
                Debug.LogAssertion("Connection to server failed: signaling message was null!");
                WidgetFactory.Instance.CreateToastrWidget(
                    $"Connection to server failed: can't reach the server", 2, "Server error");
                yield break;
            }

            StartCoroutine(SetDesc(Side.Remote, resMsg.ToDesc()));
        }

        private bool connectionStarted = false;
        private bool anyIce = false;


        void StartConnection()
        {
            if (connectionStarted)
            {
                Debug.LogWarning("Connection already started! Not reconnecting");
                return;
            }
            connectionStarted = true;
            if (UsingSocket)
            {
                if (socket.robot_name != null)
                {
                    Debug.Log("offering connection!");
                    socket.OfferConnection();
                }
                else
                {
                    Debug.LogError("no robot to operate!");
                }
            }
            else
            {
                var msg = new SignalingMsg
                {
                    type = pc.LocalDescription.type.ToString().ToLower(),
                    sdp = pc.LocalDescription.sdp
                };

                StartCoroutine(aiortcSignaling(msg));
            }
        }

        /// <summary>
        /// Webrtc frame updates are listened here and for every incoming texture it is setting spherical game object texture with coming frames
        /// Frames can be also transfered to ImtpEncoder class and can be manipulated further, currently disabled because it is causing rendering 
        /// problems on Oculus 
        /// </summary>
        void Update()
        {
            if (anyIce && DateTime.Now - lastIceCandidateTime > TimeSpan.FromSeconds(2) && !connectionStarted)
            {
                Debug.Log("No ICE candidate for a while -> starting connection");
                StartConnection();
            }

            timeElapsed += Time.deltaTime;

            if (timeElapsed > publishMessageFrequency && RobotConnected)
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
            Debug.Log($"robot {robotName} connected");
            RobotConnected = true;
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