using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Training
{
    public class AvatarNavigation : MonoBehaviour
    {
        public Vector3 target;
        private Transform wheelchair;
        [SerializeField, Range(0, 10)] private float attractSpeed = 3;
        [SerializeField, Range(0, 1)] private float detractSpeed = 0.06f;

        // Start is called before the first frame update
        void Start()
        {
            target = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 detract = Vector3.zero;
            if (wheelchair == null)
            {
                try
                {
                    wheelchair = WheelchairStateManager.Instance.gameObject.transform;
                }
                catch (System.NullReferenceException) { }
            }
            else
            {
                detract = target - wheelchair.position;
                float mag = detract.magnitude;
                detract /= mag * mag;
                // project to xz plane while keeping it's length
                detract = Vector3.ProjectOnPlane(detract, Vector3.up).normalized * mag;

            }
            transform.position = Vector3.MoveTowards(transform.position, target, attractSpeed * Time.deltaTime)
                + detractSpeed * detract;
        }
    }
}
