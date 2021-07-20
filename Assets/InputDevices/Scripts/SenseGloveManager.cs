using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenseGloveManager : Singleton<SenseGloveManager>
{
    public GameObject leftGlove, rightGlove;
    public JointTransfer.AngleRangeStore leftAngleRangeStore, rightAngleRangeStore;
    //public Training.Calibration.HandCalibrator leftCalibrator = null, rightCalibrator = null;

    [Header("Debug Object Drawing")]
    public bool drawDebugSpheres = false;

    [SerializeField] private GameObject[] leftHandJoints, rightHandJoints;
    [SerializeField] private float scale = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        if (drawDebugSpheres)
        {
            foreach (var joints in new GameObject[][] { leftHandJoints, rightHandJoints })
            {
                foreach (var joint in joints)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    obj.transform.position = Vector3.zero;
                    obj.transform.localScale = new Vector3(scale, scale, scale);
                    obj.transform.SetParent(joint.transform, false);
                }
            }
        }
        //StartCoroutine(FindLeftCalibrator());
        //StartCoroutine(FindRightCalibrator());
    }

    //private IEnumerator FindLeftCalibrator()
    //{
    //    while (leftCalibrator == null)
    //    {
    //        FindCalibrator(ref leftCalibrator, "LeftHandCalibrator");
    //        yield return new WaitForFixedUpdate();
    //    }
    //}

    //private IEnumerator FindRightCalibrator()
    //{
    //    while (rightCalibrator == null)
    //    {
    //        FindCalibrator(ref rightCalibrator, "RightHandCalibrator");
    //        yield return new WaitForFixedUpdate();
    //    }
    //}

    //private void FindCalibrator(ref Training.Calibration.HandCalibrator calibrator, string tag)
    //{
    //    var obj = GameObject.FindGameObjectWithTag(tag);
    //    if (obj != null)
    //    {
    //        calibrator = obj.GetComponent<Training.Calibration.HandCalibrator>();
    //    }
    //}
}
