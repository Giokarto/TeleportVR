using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Training
{
    public class AvatarNavigation : MonoBehaviour
    {
        public Vector3 target;
        [Range(0, 10)] public float speed = 3;

        // Start is called before the first frame update
        void Start()
        {
            target = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        }
    }

}
