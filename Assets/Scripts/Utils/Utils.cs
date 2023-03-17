using UnityEngine;

public static class GameObjectUtils
{
    public static void ChangeLayerRecursively(GameObject root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }
}