using UnityEngine;

namespace ServerConnection.Aiortc
{
    public static class WebRTCMessages
    {
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
}