using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentToWheelchair : MonoBehaviour
{
    private bool parented;
    // Start is called before the first frame update
    void Start()
    {
        parented = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!parented)
        {
            try
            {
                transform.SetParent(WheelchairStateManager.Instance.gameObject.transform, true);
                parented = true;
            }
            catch (System.Exception)
            {

            }
        }
    }
}
