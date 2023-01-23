using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RosUtils : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Vector3 Vector2Ros(Vector3 vector3)
    {
        return new Vector3(vector3.z, -vector3.x, vector3.y);
    }

    public static Quaternion Quaternion2Ros(Quaternion quaternion)
    {
        return new Quaternion(-quaternion.z, quaternion.x, -quaternion.y, quaternion.w);
    }
}
