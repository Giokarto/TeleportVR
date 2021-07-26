using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Training
{
    public class CollisionTrigger : MonoBehaviour
    {

        public string requiredTag;
        public float stationaryThreshold = 0.01f;
        private Callbacks<float> onTriggerEnter = new Callbacks<float>() , onTriggerExit = new Callbacks<float>();
        private Vector3 previousPosition;

        void Start()
        {
        }

        void FixedUpdate()
        {
            previousPosition = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                onTriggerEnter.Call((previousPosition - transform.position).magnitude);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                onTriggerExit.Call((previousPosition - transform.position).magnitude);
            }
        }

        public void TriggerEnterCallback(System.Action<float> callback, bool once = false) => onTriggerEnter.Add(callback, once);

        public void TriggerExitCallback(System.Action<float> callback, bool once = false) => onTriggerEnter.Add(callback, once);
    }
}
