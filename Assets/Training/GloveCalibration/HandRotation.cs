using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRotation : MonoBehaviour
{

    public enum Axis { X, Y, Z }
    public string tag, name;
    public Axis[] flipAxes;

    [SerializeField, Range(0,1)] private float rotationMultiplier = 1;
    [SerializeField]private Transform target = null;
    private GameObject newParent;
    private bool lastActiveSelf;
    private Quaternion initialSelfRotation, initialTargetRotation;
    private Transform oldParent;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
        {
            if (obj.name == name)
            {
                target = obj.transform;
            }
        }
        if (target == null)
        {
            Debug.LogWarning($"Hand Rotation not Initialized: Could not find object with tag: {tag}, name: {name}");
            return;
        }
        newParent = new GameObject();
        oldParent = transform.parent;
    }

    void Init()
    {
        initialSelfRotation = transform.rotation;
        initialTargetRotation = target.rotation;
        newParent.transform.SetParent(oldParent);
        newParent.transform.SetPositionAndRotation(transform.position, target.rotation);
        transform.SetParent(newParent.transform, true);
    }

    void DeInit()
    {
        transform.rotation = initialSelfRotation;
        transform.SetParent(oldParent, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf && !lastActiveSelf)
        {
            Init();
        }
        else if (!gameObject.activeSelf && lastActiveSelf)
        {
            DeInit();
        }

        if (target != null)
        {
            Quaternion diffRotation = target.rotation * Quaternion.Inverse(initialTargetRotation);
            diffRotation.ToAngleAxis(out float angle, out Vector3 axis);
            foreach (var a in flipAxes)
            {
                switch(a)
                {
                    case Axis.X:
                        axis.x *= -1;
                        break;
                    case Axis.Y:
                        axis.y *= -1;
                        break;
                    case Axis.Z:
                        axis.z *= -1;
                        break;
                }
            }
            newParent.transform.rotation = Quaternion.AngleAxis(angle * rotationMultiplier, axis) * initialTargetRotation;
        }

        lastActiveSelf = gameObject.activeSelf;
    }
}
