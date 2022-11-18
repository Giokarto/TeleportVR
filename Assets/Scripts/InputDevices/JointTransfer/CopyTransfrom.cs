using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputDevices.JointTransfer
{
    public class CopyTransfrom : MonoBehaviour
    {
        [Tooltip("GameObject to copy from")]
        public Transform controller;
        [Tooltip("Whether to copy the global position from the controller")]
        public bool position = false;
        [Tooltip("Whether to copy the global rotation from the controller")]
        public bool rotation = true;

        public Quaternion rotationOffset = Quaternion.identity;
        public Vector3 positionOffset = Vector3.zero;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (controller != null && position)
            {
                transform.position = positionOffset + controller.position;
            }
            if (controller != null && rotation)
            {
                transform.rotation = controller.rotation;
                transform.localRotation *= rotationOffset;
            }

        }
    }

}
