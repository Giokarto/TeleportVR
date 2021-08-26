using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class StereoPlaneMover : MonoBehaviour
{
    //public Transform leftImage, rightImage;
    //public float operatorPupilDistance = 63;
    //private Vector3 leftInitPos, rightInitPos;
    //[SerializeField] private float calibratedEyeDistance = 60;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    leftInitPos = leftImage.localPosition;
    //    rightInitPos = rightImage.localPosition;
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    var center = (leftInitPos + rightInitPos) / 2;
    //    var factor = operatorPupilDistance / calibratedEyeDistance;
    //    var l_dir = (leftInitPos - center) * factor;
    //    l_dir.y = 0;
    //    l_dir.z = 0;
    //    var r_dir = (rightInitPos - center) * factor;
    //    r_dir.y = 0;
    //    r_dir.z = 0;
    //    leftImage.localPosition = center + l_dir;
    //    rightImage.localPosition = center + r_dir;
    //}
    public Transform leftImage, rightImage;
    public float horizontal = 1;
    public float vertical = 1;
    public float diagonal = 1;
    private Vector3 leftInitPos, rightInitPos;
    [SerializeField] private float calibratedEyeDistance = 60;

    // Start is called before the first frame update
    void Start()
    {
        leftInitPos = leftImage.localPosition;
        rightInitPos = rightImage.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        var center = (leftInitPos + rightInitPos) / 2;
        var l_dir = (leftInitPos - center);
        var r_dir = (rightInitPos - center);
        leftImage.localPosition = center + new Vector3(l_dir.x * horizontal, l_dir.y * vertical, 0);
        rightImage.localPosition = center + new Vector3(r_dir.x * horizontal, r_dir.y * vertical, 0);
    }
}
