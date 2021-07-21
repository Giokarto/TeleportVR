using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Training
{
    public class CollisionTrigger : MonoBehaviour
    {

        public string requiredTag;
        private Dictionary<System.Action, bool> onTriggerEnter, onTriggerExit;

        void Start()
        {
            onTriggerEnter = new Dictionary<System.Action, bool>();
            onTriggerExit = new Dictionary<System.Action, bool>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                Call(ref onTriggerEnter);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.CompareTag(requiredTag))
            {
                Call(ref onTriggerExit);
            }
        }

        private void Call(ref Dictionary<System.Action, bool> callbacks)
        {
            List<System.Action> toRemove = new List<System.Action>();
            List<System.Action> toCall = new List<System.Action>(callbacks.Keys);
            // three interations to make sure the collection is not modified while iterating 
            foreach (var entry in callbacks)
            {
                if (entry.Value) toRemove.Add(entry.Key);
            }
            foreach(var key in toRemove)
            {
                callbacks.Remove(key);
            }
            foreach(var func in toCall)
            {
                func();
            }
        }

        public void TriggerEnterCallback(System.Action callback, bool once = false)
        {
            onTriggerEnter[callback] = once;
        }

        public void TriggerExitCallback(System.Action callback, bool once = false)
        {
            onTriggerExit[callback] = once;
        }
    }
}
