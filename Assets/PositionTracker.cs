using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    public GameObject objectToTrack;
    public Vector3 positionOffset = new Vector3(0.0f,-0.8f,0.0f);
    public Vector3 orinetationOffset = new Vector3(-0.2f, 0.0f, 0.0f);
    // Start is called before the first frame update
    void Start()
    {
        if (objectToTrack == null)
        {
            objectToTrack = GameObject.Find("Wheelchair");

            Debug.Log("Found wheelchair object: " + objectToTrack.name);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        ////var pos = objectToTrack.transform.position;
        //var orn = objectToTrack.transform.rotation.eulerAngles;
        //orn.z = 0;
        ////orn.x = 0;
        ////orn += orinetationOffset;
        ////orn.x -= 0.2f;
        ////orn.z = 0;
        ////pos.y -= 0.8f;

        //transform.position = objectToTrack.transform.position + positionOffset;
        //transform.rotation = Quaternion.Euler(orn);        ////var pos = objectToTrack.transform.position;
        //var orn = objectToTrack.transform.rotation.eulerAngles;
        //orn.z = 0;
        ////orn.x = 0;
        ////orn += orinetationOffset;
        ////orn.x -= 0.2f;
        ////orn.z = 0;
        ////pos.y -= 0.8f;

        //transform.position = objectToTrack.transform.position + positionOffset;
        //transform.rotation = Quaternion.Euler(orn);

  
    }
}
