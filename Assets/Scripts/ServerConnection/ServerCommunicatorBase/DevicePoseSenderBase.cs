using RosMessageTypes.Geometry;
using ServerConnection.RosTcpConnector;
using UnityEngine;

namespace ServerConnection.ServerCommunicatorBase
{
    public abstract class DevicePoseSenderBase : MonoBehaviour
    {
        
        private PoseMsg poseMsg;
        private Vector3Msg velMsg;
        private Vector3Msg accMsg;

        public PoseMsg GetLatestDevicePose(UnityEngine.XR.InputDevice device)
        {

            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 devicePosition);
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion deviceRotation);
            var rosPosition = RosUtils.Vector2Ros(devicePosition);
            var rosOrientation = RosUtils.Quaternion2Ros(deviceRotation);

            poseMsg = new PoseMsg();

            poseMsg.position.x = rosPosition.x;
            poseMsg.position.y = rosPosition.y;
            poseMsg.position.z = rosPosition.z;

            poseMsg.orientation.x = rosOrientation.x;
            poseMsg.orientation.y = rosOrientation.y;
            poseMsg.orientation.z = rosOrientation.z;
            poseMsg.orientation.w = rosOrientation.w;

            return poseMsg;

        }

        public Vector3Msg GetLatestDeviceVelocity(UnityEngine.XR.InputDevice device)
        {
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
            var rosVelocity = RosUtils.Vector2Ros(deviceVelocity);
            velMsg = new Vector3Msg();
            velMsg.x = rosVelocity.x;
            velMsg.y = rosVelocity.y;
            velMsg.z = rosVelocity.z;

            return velMsg;
        }

        public Vector3Msg GetLatestDeviceAcceleration(UnityEngine.XR.InputDevice device)
        {
            device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceAcceleration, out Vector3 deviceAcceleration);
            var rosAccelearation = RosUtils.Vector2Ros(deviceAcceleration);
            accMsg = new Vector3Msg();
            accMsg.x = rosAccelearation.x;
            accMsg.y = rosAccelearation.y;
            accMsg.z = rosAccelearation.z;

            return accMsg;
        }
    }
}