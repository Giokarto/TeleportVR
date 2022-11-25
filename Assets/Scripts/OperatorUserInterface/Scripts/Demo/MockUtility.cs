using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MockUtility : MonoBehaviour
{
#if ROSSHARP
    internal RosJsonPublisher rosJsonPublisher;

    // Start is called before the first frame update
    void Start()
    {
        rosJsonPublisher = GameObject.FindGameObjectWithTag("RosManager").GetComponent<RosJsonPublisher>();
    }
#endif
}
