using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceExpressionReader : MonoBehaviour
{
    public GameObject cube;
    OVRFaceExpressions faceExpressions;
    OVREyeGaze eyeGaze;
    float prevX, prevY;

    float weight;
    // Start is called before the first frame update
    void Start()
    {
        faceExpressions = gameObject.GetComponent<OVRFaceExpressions>();
        eyeGaze = gameObject.GetComponent<OVREyeGaze>();
        prevX = 0;
        prevY = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (faceExpressions == null)
        {
            Debug.LogError("Could not find OVRFaceExpressions");
            return;
        }
        faceExpressions.TryGetFaceExpressionWeight(OVRFaceExpressions.FaceExpression.BrowLowererL, out weight);
        cube.transform.rotation = eyeGaze.transform.rotation;

        //Debug.Log($"{OVRFaceExpressions.FaceExpression.BrowLowererL} weight is: {weight}");
    }
}
