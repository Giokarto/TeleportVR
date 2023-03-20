using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ServerConnection.Aiortc
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

    public class JointPoseMessage
    {
        private Dictionary<string, float> jointPoses = new Dictionary<string, float>();

        public void Add(string jointName, float jointValue)
        {
            jointPoses.Add(jointName, jointValue);
        }
        
        public string ToJson()
        {
            return JsonConvert.SerializeObject(jointPoses);
        }
    }
}