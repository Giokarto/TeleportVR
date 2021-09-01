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
        private Vector3 prev1, prev2;

        void Start()
        {
        }

        void FixedUpdate()
        {
            prev2 = prev1;
            prev1 = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                onTriggerEnter.Call((prev2 - transform.position).magnitude / Time.fixedDeltaTime);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                onTriggerExit.Call((prev2 - transform.position).magnitude / Time.fixedDeltaTime);
            }
        }

        public void TriggerEnterCallback(System.Action<float> callback, bool once = false) => onTriggerEnter.Add(callback, once);

        public void TriggerExitCallback(System.Action<float> callback, bool once = false) => onTriggerEnter.Add(callback, once);
    }
}
