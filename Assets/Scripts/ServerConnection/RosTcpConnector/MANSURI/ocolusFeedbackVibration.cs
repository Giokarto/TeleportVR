/*using UnityEngine;
using System.Collections;
using Oculus;
public class ocolusFeedbackVibration : MonoBehaviour
{
    public float proximityThreshold = 1.0f; // Adjust this value according to your needs

    private void Update()
    {
        // Check the distance between the object and the user's position or hand controllers
        float distance = Vector3.Distance(transform.position, OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));

        if (distance < proximityThreshold)
        {
            // Trigger vibration feedback when the user is close to the object
            OVRInput.SetControllerVibration(1.0f, 1.0f, OVRInput.Controller.RTouch);
        }
        else
        {
            // Stop vibration feedback when the user is not close to the object
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        }
    }
}*/
