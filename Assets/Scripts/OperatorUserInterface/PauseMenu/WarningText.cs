using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PauseMenu
{
    public class WarningText : MonoBehaviour
    {

        [Range(0, 1)] public float scaleFactor = 0.1f;
        [Range(0, 1)] public float timeFactor = 0.5f;

        private Vector3 initScale;
        // Start is called before the first frame update
        void Start()
        {
            initScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            transform.localScale = initScale * (1 + scaleFactor * Mathf.Sin(Time.fixedTime * timeFactor));
        }
    }
}
