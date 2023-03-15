using InputDevices.VRControllers;
using UnityEngine;

public class HeadRotationController : MonoBehaviour
{
    private GameObject leftEyeSphere;
    private GameObject rightEyeSphere;

    private void Start()
    {
        if (leftEyeSphere == null)
        {
            leftEyeSphere = GameObject.Find("MinusYCut(Clone)");
            Debug.Log("dannyb found left eye");
        }
        else
        {
            Debug.Log("dannyb cant find left eye");
        }

        if (rightEyeSphere == null)
        {
            rightEyeSphere = GameObject.Find("PlusYCut(Clone)");
            Debug.Log("dannyb found right eye");
        }
        else
        {
            Debug.Log("dannyb cant find right eye");
        }
    }

    public void ProcessHeadMessage(Vector3 headPosition)
    {
        var device = VRControllerInputSystem.GetDeviceByName("headset/");
        device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 devicePosition);
        Quaternion targetRotation = Quaternion.LookRotation(headPosition - devicePosition, Vector3.up);
        
        leftEyeSphere.transform.rotation = targetRotation;
        rightEyeSphere.transform.rotation = targetRotation;
    }
}
