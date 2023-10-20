using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MinimapController : MonoBehaviour
{
    [SerializeField]
    public GameObject Minimap;  
    public GameObject WarningImage;
    public GameObject pcl;

    private void Awake()
    {
        // This ensures the GameObject this script is attached to is not destroyed when loading a new scene
        DontDestroyOnLoad(gameObject);
        
        // Add a listener to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Training")
        {
            Minimap.SetActive(false);  
            WarningImage.SetActive(false);  
            pcl.SetActive(false);  

        }
        else if (scene.name == "Main")
        {
            Minimap.SetActive(true);  
            WarningImage.SetActive(true);  
            pcl.SetActive(true);          }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
