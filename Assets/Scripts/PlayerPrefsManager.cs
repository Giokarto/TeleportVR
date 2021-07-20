using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerPrefsManager : MonoBehaviour
{
    public bool clearPrefs = false;

    // Update is called once per frame
    void Update()
    {
        if (clearPrefs)
        {
            clearPrefs = false;
            PlayerPrefX.DeleteAll();
            Debug.Log("Cleared all player prefs");
        }
    }
}
