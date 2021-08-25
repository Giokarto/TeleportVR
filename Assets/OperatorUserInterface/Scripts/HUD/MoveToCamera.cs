using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class MoveToCamera : MonoBehaviour
{

    public float slerpSpeed = 10f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            transform.position = Camera.main.transform.position;
            RotateYAxis(Camera.main.transform.rotation);
        }
        catch (System.NullReferenceException) { }
    }

    private void RotateYAxis(Quaternion target)
    {
        var yRot = new Vector3(0, target.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, 
            Quaternion.Euler(yRot), 
            Mathf.Clamp01(Time.deltaTime * slerpSpeed));
    }
}
