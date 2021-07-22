using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Training
{
    public class CollisionTrigger : MonoBehaviour
    {

        public string requiredTag;
        private Callbacks<string> onTriggerEnter, onTriggerExit;

        void Start()
        {
            onTriggerEnter = new Callbacks<string>();
            onTriggerExit = new Callbacks<string>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                onTriggerEnter.Call(requiredTag);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                onTriggerExit.Call(requiredTag);
            }
        }

        public void TriggerEnterCallback(System.Action<string> callback, bool once = false) => onTriggerEnter.Add(callback, once);

        public void TriggerExitCallback(System.Action<string> callback, bool once = false) => onTriggerEnter.Add(callback, once);
    }
}
