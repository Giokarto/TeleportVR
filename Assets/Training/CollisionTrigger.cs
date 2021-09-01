using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Training
{
    public class CollisionTrigger : MonoBehaviour
    {

        public string requiredTag;
        public float stationaryThreshold = 0.01f;
        private Callbacks<Vector3> onTriggerEnter = new Callbacks<Vector3>() , onTriggerExit = new Callbacks<Vector3>();

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                onTriggerEnter.Call(transform.position);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                onTriggerExit.Call(transform.position);
            }
        }

        public void TriggerEnterCallback(System.Action<Vector3> callback, bool once = false) => onTriggerEnter.Add(callback, once);

        public void TriggerExitCallback(System.Action<Vector3> callback, bool once = false) => onTriggerEnter.Add(callback, once);
    }
}
