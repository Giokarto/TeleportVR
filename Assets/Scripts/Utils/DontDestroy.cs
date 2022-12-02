using System;
using UnityEngine;

/// <summary>
/// Object won't be destroyed on the load of a new scene. Useful for input components, UI display etc.
/// </summary>
public class DontDestroy : MonoBehaviour
{
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
