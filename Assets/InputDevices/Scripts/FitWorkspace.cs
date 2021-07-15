using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitWorkspace : MonoBehaviour
{
    public Transform body, head, controllers, leftHand, leftHandObjective, leftEye;

    void Update()
    {
        if (Input.GetKey(KeyCode.F))
        {
            Fit();
        }
    }

    /// <summary>
    /// Scales this object such that the target x positon matches the hands
    /// </summary>
    public void Fit()
    {
        float factor = leftHandObjective.position.x / leftHand.position.x;
        Debug.Log($"Fitting workspace with factor {factor}");
        // scale objects
        Transform headParent = head.parent;
        Vector3 leftEyePos = leftEye.position;
        head.SetParent(body, true);
        body.localScale *= factor;
        head.SetParent(headParent, true);
        controllers.position = new Vector3(controllers.position.x, controllers.position.y - leftEye.position.y + leftEyePos.y, controllers.position.z);
    }
}
