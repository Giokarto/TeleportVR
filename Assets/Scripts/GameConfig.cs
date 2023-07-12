using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteAlways]
public class GameConfig : Singleton<GameConfig>
{
    
    [System.Serializable]
    public class PlayerSettings
    {
        [Range(40, 90)]
        public float OperatorIPD = 63;
        public bool OperatorManualOverwrite = false;
        public float OperatorHorizontal = 0;
        public float OperatorVertical = 0.1432849f;//0;
        public string RobotName = null;
        public string RosIP = "10.7.0.3";
    }


    public static string configName = "config.json";
    private string path;
    public PlayerSettings settings;


    void Awake()
    {
        path = Application.persistentDataPath + "/" + configName;
        //Debug.Log("Game config awake");
        settings = new PlayerSettings();
        try
        {
            var s = File.ReadAllText(path);
            settings = JsonUtility.FromJson<PlayerSettings>(s);
            Debug.Log($"Successfully loaded settings from {path}:\n {settings.RosIP}");
            //Debug.Log(settings.RosIP);
        }
        catch (IOException)
        {
            Debug.LogError($"Coud not read settings from {path}, using default");
            WriteSettings();
        }
    }

    public void WriteSettings()
    {
        var json = JsonUtility.ToJson(settings, prettyPrint: true);
        //Debug.Log($"Wrote GameSettings to: {path}");
        //Debug.Log(json);
        File.WriteAllText(path, json);
    }

    private void OnApplicationQuit()
    {
        WriteSettings();
    }
}
