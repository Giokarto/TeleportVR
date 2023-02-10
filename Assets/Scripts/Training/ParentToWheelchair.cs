using System.Collections;
using System.Collections.Generic;
using RobodyControl;
using UnityEngine;


namespace Training
{
    public class ParentToWheelchair : MonoBehaviour
    {
        private Transform wheelchair;

        // Update is called once per frame
        void Update()
        {
            if (wheelchair == null)
            {
                try
                {
                    wheelchair = WheelchairStateManager.Instance.gameObject.transform;
                }
                catch (System.NullReferenceException)
                {
                    return;
                }
                transform.SetParent(wheelchair);
            }
        }
    }
}

