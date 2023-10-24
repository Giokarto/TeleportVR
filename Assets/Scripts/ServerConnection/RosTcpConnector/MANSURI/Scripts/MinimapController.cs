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
    public GameObject Mirror;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Mirror == null) // only find the Mirror object
        {
            Mirror = GameObject.Find("Mirror");
            
            if (Mirror == null)
            {
                Debug.LogError("Mirror GameObject not found in the scene.");
            }
        }
    }

    private void Update()
    {
        if (Mirror != null && Mirror.activeSelf)
        {
            DeactivateObjects();
        }
        else
        {
            ActivateObjects();
        }
    }

    private void ActivateObjects()
    {
        if (WarningImage != null) WarningImage.SetActive(true);
        if (pcl != null) pcl.SetActive(true); 
        if (Minimap != null) Minimap.SetActive(true);
    }

    private void DeactivateObjects()
    {
        if (WarningImage != null) WarningImage.SetActive(false);
        if (pcl != null) pcl.SetActive(false);
        if (Minimap != null) Minimap.SetActive(false);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  // Unsubscribe from the event when the GameObject is destroyed
    }
}