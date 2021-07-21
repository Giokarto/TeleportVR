using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentPosOffset : MonoBehaviour
{
    public Transform wheelchair;
    public Vector3 offset;
    private bool init = false;

    void Update()
    {
        if (!init)
        {
            try
            {
                wheelchair = WheelchairStateManager.Instance.gameObject.transform;
            }
            catch (System.NullReferenceException)
            {
                return;
            }
            init = true;
        }

        // It's camera position
        transform.position = wheelchair.position + offset;
    }
}
