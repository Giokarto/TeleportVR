using System.Collections;
using System.Collections.Generic;
using InputDevices.VRControllers;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class FollowHeadset : MonoBehaviour
{
    // Reference to the main camera (which represents the VR headset)
    public Camera mainCamera;

    // Rotation speed
    public float speed = 1.0f;
    
    void Update()
    {
        // Calculate the rotation needed to look at the VR camera
        Quaternion targetRotation = Quaternion.LookRotation(mainCamera.transform.position - transform.position);

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
    }
}

