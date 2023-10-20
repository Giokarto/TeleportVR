using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowHeadset : MonoBehaviour
{
    // Reference to the main camera (which represents the VR headset)
    public Camera mainCamera;

    // Rotation speed
    public float speed = 2.5f; // Increased speed for faster rotation
    public float rotationMultiplier = 1.5f;


    private void Awake()
    {
        // Assuming the mainCamera is attached to the same GameObject as this script
        // If not, you can adjust this line accordingly
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera is not assigned!");
            return;
        }
        
        if (mainCamera != null)
        {
            // Get the yaw rotation of the VR headset
            float targetYRotation = mainCamera.transform.localEulerAngles.y;

            // Adjusted rotation mapping for better matching with the VR headset's movement
            float targetZRotation = -targetYRotation * rotationMultiplier; 

            // Rotate the 2D GameObject around the Z-axis to match the yaw rotation of the VR headset
            float zRotation = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, speed * Time.deltaTime);

            // Set only the Z rotation, keeping X and Y at 0
            transform.localEulerAngles = new Vector3(0f, 0f, zRotation);
        }
    }
}
