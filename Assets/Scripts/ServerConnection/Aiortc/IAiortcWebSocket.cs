using Unity.WebRTC;

namespace ServerConnection.Aiortc
{
    public interface IAiortcWebSocket
    {
        void OnWebRtcConfigGathered(string[] urls);
        void OnRobotConnected(string robotName);
        RTCPeerConnection GetPeerConnection();
        void SetRemoteDescription(ref RTCSessionDescription rtcSessionDescription);
    }
}