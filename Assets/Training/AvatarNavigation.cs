using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarNavigation : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private Vector3 target;
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

    public void UpdateTarget(Vector3 _target) => target = _target;
}
