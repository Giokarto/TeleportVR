using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameConfig : Singleton<GameConfig>
{
    [System.Serializable]
    public class PlayerSettings
    {
        [Range(20, 120)]
        public float OperatorIPD = 63;
    }

    public string configName = "config.json";
    private string path;
    public PlayerSettings settings;

    void Awake()
    {
        path = Application.persistentDataPath + "/" + configName;
        settings = new PlayerSettings();
        try
        {
            var s = File.ReadAllText(path);
            settings = JsonUtility.FromJson<PlayerSettings>(s);
            Debug.Log($"Successfully loaded settings from {path}");
        }
        catch (IOException)
        {
            Debug.LogError($"Coud not read settings from {path}, using default");
            WriteSettings();
        }
    }

    private void WriteSettings()
    {
        var json = JsonUtility.ToJson(settings);
        Debug.Log($"Wrote GameSettings to: {path}");
        File.WriteAllText(path, json);
    }

    private void OnDestroy()
    {
        WriteSettings();
    }
}
