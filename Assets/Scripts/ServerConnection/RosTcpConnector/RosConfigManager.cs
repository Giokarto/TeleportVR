using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;


public class RosConfigManager : MonoBehaviour
{

    public bool UseConfigFile = true;
    
    void Start()
    {
        if (GameConfig.Instance.settings.RosIP != "" && UseConfigFile)
            ROSConnection.GetOrCreateInstance().RosIPAddress = GameConfig.Instance.settings.RosIP;
    }

    void Update()
    {
       
    }

    
}
