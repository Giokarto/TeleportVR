using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class MoveToCamera : MonoBehaviour
{
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
        }
        catch (System.NullReferenceException) { }
    }
}
