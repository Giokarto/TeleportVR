using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRotation : MonoBehaviour
{
    public string tag, name;

    private Transform target = null;
    private GameObject newParent;
    private bool lastActiveSelf;
    private Quaternion initialSelfRotation;
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
        Debug.Log("Hand Rotation Init");
        initialSelfRotation = transform.rotation;
        newParent.transform.SetParent(oldParent);
        newParent.transform.position = transform.position;
        newParent.transform.rotation = target.rotation;
        transform.SetParent(newParent.transform, true);
    }

    void DeInit()
    {
        Debug.Log("Hand Rotation DeInit");
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
            newParent.transform.rotation = target.rotation;
        }

        lastActiveSelf = gameObject.activeSelf;
    }
}
