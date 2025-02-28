using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.WebRTC;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// This class will be ran by its own and will set parameters such as stun_urls and robot name which later be used in aiortcConnector.
    /// We are creating the connection as soon as the app launches.
    /// </summary>
    public class AiortcWebSocket
    {
        public string URL = "ws://83.229.87.110:8080";
        private IAiortcWebSocket aiortcWebSocketInterface;
        private WebSocket ws;
        public string robot_name;
        public string[] stun_urls;

        public AiortcWebSocket(IAiortcWebSocket aiortcWebSocketInterface)
        {
            this.aiortcWebSocketInterface = aiortcWebSocketInterface;
            Start();
        }

        void Start()
        {
            if (aiortcWebSocketInterface == null)
            {
                Debug.LogError("can not find aiortc connector");
            }
            Debug.Log("open AiortcWebSocket");
            // Create a new WebSocket instance
            ws = new WebSocket(URL,"json");

            // Register event handlers for the WebSocket instance
            ws.OnOpen += OnWebSocketOpen;
            ws.OnMessage += OnWebSocketMessage;
            ws.OnError += OnWebSocketError;
            ws.OnClose += OnWebSocketClose;

            // Start the WebSocket connection
            ws.Connect();
        }

        void OnWebSocketOpen(object sender, System.EventArgs e)
        {
            Debug.Log("WebSocket connection opened.");

            // Send a test message to the server
            //ws.Send("Hello, server!");
        }

        void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            JObject jsonObject = JObject.Parse(e.Data);
            if (jsonObject["type"] != null)
            {
                switch ((string)jsonObject["type"])
                {
                    case "id":
                        Debug.Log("WebSocket incoming id: " + (string)jsonObject["id"]);
                        WebSocketUserMessage message = new WebSocketUserMessage { type = "username", name = "operator/gorgeous_operator" };
                        string json = JsonConvert.SerializeObject(message);
                        ws.Send(json);
                        break;
                    case "webrtc_config":
                        OnWebrtcConfig(e.Data);
                        break;
                    case "userlist":
                        OnUserList(((JArray)jsonObject["users"])?.ToObject<List<string>>());
                        break;
                    case "answer":
                        Debug.Log("WebSocket incoming id: " + e.Data);
                        OnOfferAccepted((string)jsonObject["sdp"]);
                        break;
                    default:
                        Debug.Log("Unknown message: " + e.Data);
                        break;
                }
            }
        }

        private void OnWebrtcConfig(string eData)
        {
            try
            {
                JObject jsonObject = JObject.Parse(eData);
                List<string> stringList = new List<string>();
                foreach (var stun in jsonObject["config"]["stun"])
                {
                    stringList.Add((string)stun);
                    Debug.Log("WebRtcConfigJson stun: " + stun);
                }
                foreach (var turn in jsonObject["config"]["turn"])
                {
                    stringList.Add((string)turn);
                    Debug.Log("WebRtcConfigJson turn: " + turn);
                }

                stun_urls = stringList.ToArray();
                aiortcWebSocketInterface.OnWebRtcConfigGathered(stun_urls);
            }
            catch (Exception e)
            {
                Debug.LogError("" + e);
                throw;
            }
        }

        /*
        private string[] OnWebrtcConfig(JsonData jToken)
        {
        }
*/
        void OnWebSocketError(object sender, ErrorEventArgs e)
        {
            Debug.LogError("WebSocket error: " + e.Message);
        }

        void OnWebSocketClose(object sender, CloseEventArgs e)
        {
            Debug.Log("WebSocket connection closed with reason: " + e.Reason);
        }

        void OnConnect(string[] urls)
        {
            //aiortcConnector.ConnectWithUrls(urls);
        }
        
        void OnUserList(List<string> users)
        {
            Debug.Log("user list ");

            foreach (var user in users)
            {
                Debug.Log("user: " + user);
                //get the first occurrence of "robot" robot
                if (user.StartsWith("robot"))
                {
                    //connection is now possible with robot name
                    robot_name = user;
                    return;
                }
            }
            //no robot is find in the current users, NO CONNECTION case can be handled here
        }

        public void OfferConnection()
        {
            try
            {
                if (robot_name == null)
                {
                    Debug.Log("No robot to operate!");
                    return;
                }
                if (aiortcWebSocketInterface.GetPeerConnection()!= null && aiortcWebSocketInterface.GetPeerConnection().LocalDescription.sdp != null)
                {
                    Debug.Log("sending signaling message");
                    WebSocketOfferMessage message = new WebSocketOfferMessage
                        { type = "offer", target = robot_name, sdp = aiortcWebSocketInterface.GetPeerConnection().LocalDescription.sdp};
                    string json = JsonConvert.SerializeObject(message);
                    ws.Send(json);
                }
                else
                {
                    Debug.Log("peer connection is not accessible, will try again");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(" " + e);
                throw;
            }
        }

        void OnOfferAccepted(string sdp)
        {
            try
            {
                Debug.Log("OnOfferAccepted");
                var x = new RTCSessionDescription
                {
                    type = RTCSdpType.Answer,
                    sdp = sdp
                };
                aiortcWebSocketInterface.SetRemoteDescription(ref x);
            }
            catch (Exception e)
            {
                Debug.LogError("" + e);
                throw;
            }
        }

        void OnDestroy()
        {
            stun_urls = null;
            robot_name = null;
            // Close the WebSocket connection when the script is destroyed
            if (ws != null && ws.ReadyState == WebSocketState.Open)
            {
                ws.Close();
            }
        }
        protected class WebSocketUserMessage {
            public string type { get; set; }
            public string name { get; set; }
        }
        
        protected class WebSocketOfferMessage {
            public string type { get; set; }
            
            public string target { get; set; }
            public string sdp { get; set; }
        }
        

        [System.Serializable]
        public class WebRtcConfigJson
        {
            public string type { get; set; }
            public Config config{ get; set; }

            public string[] GetUrls()
            {
                // Create an empty list of strings
                List<string> stringList = new List<string>();

                foreach (var url in config.stun)
                {
                    stringList.Add(url);
                }

                foreach (var url in config.turn)
                {
                    stringList.Add(url);
                }

                return stringList.ToArray();
            }
            public class Config
            {
                public string[] stun{ get; set; }
                public string[] turn{ get; set; }
            }
        }

    }
}