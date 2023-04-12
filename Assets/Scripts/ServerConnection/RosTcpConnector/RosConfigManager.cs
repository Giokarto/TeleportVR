using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace ServerConnection.RosTcpConnector
{
    public class RosConfigManager : MonoBehaviour
    {

        public bool UseConfigFile = true;

        void Start()
        {
            if (GameConfig.Instance.settings.RosIP != "" && UseConfigFile)
                ROSConnection.GetOrCreateInstance().RosIPAddress = GameConfig.Instance.settings.RosIP;
            else
            {
                ROSConnection.GetOrCreateInstance().RosIPAddress = "10.1.0.6";
            }
        }

        void Update()
        {

        }


    }
}