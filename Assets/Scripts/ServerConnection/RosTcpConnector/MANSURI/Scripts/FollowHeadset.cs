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
        if (mainCamera != null)
        {
            // Get the yaw rotation of the VR headset
            float targetYRotation = mainCamera.transform.localEulerAngles.y;

            // Map this to the Z-axis rotation of the 2D GameObject
            float targetZRotation = -targetYRotation;

            // Rotate the 2D GameObject around the Z-axis to match the yaw rotation of the VR headset
            float zRotation = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, speed * Time.deltaTime);

            // Set only the Z rotation, keeping X and Y at 0
            transform.localEulerAngles = new Vector3(0f, 0f, zRotation);
        }
    }
}
