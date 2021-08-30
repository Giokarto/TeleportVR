using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchButtonActivationVolume : MonoBehaviour
{
    public Callbacks<string> enterCallbacks = new Callbacks<string>();
    public Callbacks<string> exitCallbacks = new Callbacks<string>();

    public new bool enabled = true;

    public string[] colliderTags;

    private void OnTriggerEnter(Collider other)
    {
        if (!ColliderHasTags(other) || !enabled)
            return;

        Debug.Log("pause menu activation enter");
        enterCallbacks.Call(other.tag);
    }


    private void OnTriggerExit(Collider other)
    {
        if (!ColliderHasTags(other) || !enabled)
            return;

        exitCallbacks.Call(other.tag);
    }

    private bool ColliderHasTags(Collider collider)
    {
        foreach (var tag in colliderTags)
        {
            if (collider.CompareTag(tag))
                return true;
        }
        return false;
    }
}
