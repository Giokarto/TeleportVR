using Unity.WebRTC;

namespace ServerConnection.Aiortc
{
    /// <summary>
    /// This interface is consisting of methods that websocket triggers from the classes that implements it.
    /// </summary>
    public interface IAiortcWebSocket
    {
        void OnWebRtcConfigGathered(string[] urls);
        void OnRobotConnected(string robotName);
        RTCPeerConnection GetPeerConnection();
        void SetRemoteDescription(ref RTCSessionDescription rtcSessionDescription);
    }
}