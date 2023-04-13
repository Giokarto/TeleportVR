using InputDevices.VRControllers;
using UnityEngine;

namespace ServerConnection
{
    /// <summary>
    /// This class is responsible with rotating the eye spheres according to the head position information that is coming from robot
    /// </summary>
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
                Debug.Log( "found left eye");
            }
            else
            {
                Debug.Log("cant find left eye");
            }

            if (rightEyeSphere == null)
            {
                rightEyeSphere = ServerBase.Instance.RightEye;
                Debug.Log("found right eye");
            }
            else
            {
                Debug.Log("cant find right eye");
            }

            referenceY = LocalRotationGameObject.transform.rotation.eulerAngles.y;
        }

        public void ProcessHeadMessage(Vector3 headRotation)
        {
            var newPose = new Vector3(headRotation.y,headRotation.x,headRotation.z);
            var newPoseInDegrees = new Vector3(newPose.x * Mathf.Rad2Deg, newPose.y * Mathf.Rad2Deg,
                newPose.z * Mathf.Rad2Deg);
            
            leftEyeSphere.transform.rotation = Quaternion.Euler(newPoseInDegrees);
            rightEyeSphere.transform.rotation = Quaternion.Euler(newPoseInDegrees);
        }
    }
}