using System;
using UnityEngine;

namespace ServerConnection
{
    public class LockPosition : MonoBehaviour
    {
        private Vector3 initialPosition;

        private void Start()
        {
            initialPosition = transform.position;
        }

        private void LateUpdate()
        {
            transform.position = initialPosition;
        }
    }
}