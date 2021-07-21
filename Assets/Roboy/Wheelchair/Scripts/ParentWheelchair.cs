using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentWheelchair : MonoBehaviour
{
    public Vector3 offset;
    private Transform wheelchair;
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
