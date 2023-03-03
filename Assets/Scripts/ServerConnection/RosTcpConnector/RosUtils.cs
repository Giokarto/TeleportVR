using UnityEngine;

namespace ServerConnection.RosTcpConnector
{
    public static class RosUtils
    {
        public static Vector3 Vector2Ros(Vector3 vector3)
        {
            return new Vector3(vector3.z, -vector3.x, vector3.y);
        }

        public static Quaternion Quaternion2Ros(Quaternion quaternion)
        {
            return new Quaternion(-quaternion.z, quaternion.x, -quaternion.y, quaternion.w);
        }
    }
}
