using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using VRKeys;

public class ROSConfigManager : MonoBehaviour
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
