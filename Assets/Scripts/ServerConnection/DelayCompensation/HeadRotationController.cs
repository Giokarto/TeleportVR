using InputDevices.VRControllers;
using UnityEngine;

namespace ServerConnection
{
    public class HeadRotationController : MonoBehaviour
    {
        private GameObject leftEyeSphere;
        private GameObject rightEyeSphere;
        public GameObject LocalRotationGameObject;
        public float referenceY;

        private void Start()
        {
            if (leftEyeSphere == null)
            {
                leftEyeSphere = ServerBase.Instance.LeftEye;
                Debug.Log( "dannyb found left eye");
            }
            else
            {
                Debug.Log("dannyb cant find left eye");
            }

            if (rightEyeSphere == null)
            {
                rightEyeSphere = ServerBase.Instance.RightEye;
                Debug.Log("dannyb found right eye");
            }
            else
            {
                Debug.Log("dannyb cant find right eye");
            }

            referenceY = LocalRotationGameObject.transform.rotation.eulerAngles.y;
        }

        public void ProcessHeadMessage(Vector3 headRotation)
        {
            //var device = VRControllerInputSystem.GetDeviceByName("headset/");
            //device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion deviceRotationQuaternion);
            //Vector3 deviceRotation = deviceRotationQuaternion.eulerAngles;
            //Debug.Log($"device: {deviceRotation} head: {headRotation}");
            //Quaternion targetRotation = Quaternion.LookRotation(headRotation - deviceRotation, Vector3.up);
/*            var deviceRotation = new Vector3(0, referenceY, 0);
            var targetRotation = Quaternion.LookRotation(headRotation + deviceRotation, Vector3.up);
            
            Debug.Log("dannyb ProcessHeadMessage" + targetRotation);

            
            leftEyeSphere.transform.Rotate(headRotation.y, headRotation.x, headRotation.z);
            rightEyeSphere.transform.Rotate(headRotation.y, headRotation. - 180, headRotation.z ); // right sphere is 180 degrees rotated
*/

            var newPose = new Vector3(headRotation.y,headRotation.x,headRotation.z);
            var newPoseInDegrees = new Vector3(newPose.x * Mathf.Rad2Deg, newPose.y * Mathf.Rad2Deg,
                newPose.z * Mathf.Rad2Deg);
            
            leftEyeSphere.transform.rotation = Quaternion.Euler(newPoseInDegrees);
            rightEyeSphere.transform.rotation = Quaternion.Euler(newPoseInDegrees) * Quaternion.Euler(0,180,0); // right sphere is 180 degrees rotated
        }
    }
}