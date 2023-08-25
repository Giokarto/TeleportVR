using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachCamera : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    void Start()
    {
        canvas.worldCamera = transform.parent.parent.GetComponent<Camera>();
    }

}
