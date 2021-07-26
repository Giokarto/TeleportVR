using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Callbacks<T>
{

    private Dictionary<System.Action<T>, bool> callbacks;

    public Callbacks()
    {
        callbacks = new Dictionary<System.Action<T>, bool>();
    }

    public void Call(T argument)
    {
        List<System.Action<T>> toRemove = new List<System.Action<T>>();
        List<System.Action<T>> toCall = new List<System.Action<T>>(callbacks.Keys);
        // three interations to make sure the collection is not modified while iterating 
        foreach (var entry in callbacks)
        {
            if (entry.Value) toRemove.Add(entry.Key);
        }
        foreach (var key in toRemove)
        {
            callbacks.Remove(key);
        }
        foreach (var func in toCall)
        {
            func(argument);
        }
    }

    public void Add(System.Action<T> callback, bool once = false) => callbacks[callback] = once;
    public void Clear() => callbacks.Clear();
}

