using System.Collections.Generic;
using UnityEngine;

public static class ObjectRegistry
{
    private static readonly Dictionary<string, Object> objects = new();

    // Static data survives between Play sessions when domain reload is disabled
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry() => objects.Clear();

    public static void Register(string id, Object obj)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning($"ObjectRegistry: '{obj.gameObject.name}' has an empty id and was not registered.", obj);
            return;
        }

        if (objects.TryGetValue(id, out Object existing) && existing != null && existing != obj)
        {
            Debug.LogWarning(
                $"ObjectRegistry: Duplicate id '{id}'. Already used by '{existing.gameObject.name}', " +
                $"so '{obj.gameObject.name}' was NOT registered. Change one of the ids.", obj);
            return;
        }

        objects[id] = obj;
    }

    public static void Unregister(string id, Object obj)
    {
        // Only remove if this exact instance is the registered one,
        // so a rejected duplicate being destroyed doesn't unregister the original.
        if (id != null && objects.TryGetValue(id, out Object existing) && existing == obj)
            objects.Remove(id);
    }

    public static Object Get(string id)
    {
        return id != null && objects.TryGetValue(id, out Object o) && o != null ? o : null;
    }

    public static T Get<T>(string id) where T : Object
    {
        return Get(id) as T;
    }
}