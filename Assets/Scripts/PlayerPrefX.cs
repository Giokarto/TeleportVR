using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension class of Unity's PlayerPrefs adding some convenience methods
/// </summary>
public class PlayerPrefX : PlayerPrefs
{
    // TODO: Add support for more types
    public static void SetVector3(string identifier, Vector3 obj)
    {
        SetFloat($"{identifier}.x", obj.x);
        SetFloat($"{identifier}.y", obj.y);
        SetFloat($"{identifier}.z", obj.z);
    }

    public static Vector3 GetVector3(string identifier, Vector3 defaultValue)
    {
        string x = $"{identifier}.x", y = $"{identifier}.y", z = $"{identifier}.z";
        if (!HasKey(x) || !HasKey(y) || !HasKey(z))
        {
            return defaultValue;
        }
        return new Vector3(GetFloat(x), GetFloat(y), GetFloat(z));
    }

    //public static void SetArray<T>(string identifier, IList<T> items)
    //{
    //    SetInt($"{identifier}.Count", items.Count);
    //    for (int i = 0; i < items.Count; i++)
    //    {
    //        var id = $"{identifier}[{i}]";
    //        switch (items[i])
    //        {
    //            case float f:
    //                SetFloat(id, f);
    //                break;
    //            case int j:
    //                SetInt(id, j);
    //                break;
    //            case string s:
    //                SetString(id, s);
    //                break;
    //            case Vector3 v:
    //                SetVector3(id, v);
    //                break;
    //            case IList<T> a:
    //                SetArray<T>(id, a);
    //                break;
    //            default:
    //                throw new System.NotImplementedException($"Type {typeof(T).FullName} is not yet implemented");
    //        }
    //    }
    //}

    //public static T GetArray<T>(string identifier) where T : IList
    //{
    //    int count = GetInt($"{identifier}.Count");
    //    var I = typeof(T).GetElementType();
    //    var items = new List<typeof(I) > ();
    //    T[] items = new typeof(I)[count];
    //    for (int i = 0; i < count; i++)
    //    {
    //        var id = $"{identifier}[{i}]";
    //        items[i] = default(T) switch
    //        {
    //            float _ => (T)System.Convert.ChangeType(GetFloat(id), typeof(T)),
    //            int _ => (T)System.Convert.ChangeType(GetInt(id), typeof(T)),
    //            string _ => (T)System.Convert.ChangeType(GetString(id), typeof(T)),
    //            Vector3 _ => (T)System.Convert.ChangeType(GetVector3(id), typeof(T)),
    //            IList<T> _ => (T)GetArray<T>(id),
    //            _ => throw new System.NotImplementedException($"Type {typeof(T).FullName} is not yet implemented"),
    //        };
    //    }
    //    return items;
    //}
}
