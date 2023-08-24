using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowHeadset : MonoBehaviour
{
    // Reference to the main camera (which represents the VR headset)
    public Camera mainCamera;

    // Rotation speed
    public float speed = 1.0f;

    private void Start()
    {
        mainCamera = GameObject.Find("RightEyeAnchor").GetComponent<Camera>();
    }

    void Update()
    {
        if(mainCamera != null) 
        {
            // Get the yaw rotation of the VR headset
            float targetYRotation = mainCamera.transform.eulerAngles.y;

            // Rotate the Roboy head in minimap to match the yaw rotation of the VR headset
            float yRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetYRotation, speed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, yRotation, 0);
        }
    }
}