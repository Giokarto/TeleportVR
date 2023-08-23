using System.Collections;
using System.Collections.Generic;
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
        if(mainCamera != null) 
        {
            // Calculate the direction to the VR camera
            Vector3 direction = mainCamera.transform.position - transform.position;
            
            // Compute the angle between the forward direction of the object and the direction to the VR camera
            float targetZRotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            //  Rotate towards the target Z rotation
            float zRotation = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, speed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, 0, zRotation);
        }
    }
}