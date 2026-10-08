using System.Collections.Generic;
using UnityEngine;

public static class ObjectRegistry
{
    private static readonly Dictionary<string, Object_> objects = new();

    // Static data survives between Play sessions when domain reload is disabled
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry() => objects.Clear();

    public static void Register(string id, Object_ obj)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning($"ObjectRegistry: '{obj.gameObject.name}' has an empty id and was not registered.", obj);
            return;
        }

        if (objects.TryGetValue(id, out Object_ existing) && existing != null && existing != obj)
        {
            Debug.LogWarning(
                $"ObjectRegistry: Duplicate id '{id}'. Already used by '{existing.gameObject.name}', " +
                $"so '{obj.gameObject.name}' was NOT registered. Change one of the ids.", obj);
            return;
        }

        objects[id] = obj;
    }

    public static void Unregister(string id, Object_ obj)
    {
        // Only remove if this exact instance is the registered one,
        // so a rejected duplicate being destroyed doesn't unregister the original.
        if (id != null && objects.TryGetValue(id, out Object_ existing) && existing == obj)
            objects.Remove(id);
    }

    public static Object_ Get(string id)
    {
        return id != null && objects.TryGetValue(id, out Object_ o) && o != null ? o : null;
    }

    public static T Get<T>(string id) where T : Object_
    {
        return Get(id) as T;
    }

    // Run something on every registered object
    public static void ForEach(System.Action<Object_> action)
    {
        var snapshot = new List<Object_>(objects.Values);
        foreach (var o in snapshot)
        {
            if (o == null) continue;   // destroyed since the snapshot (Unity null check)
            action(o);
        }
    }

    // Only objects of a given type, e.g. every Person
    public static void ForEach<T>(System.Action<T> action) where T : Object_
    {
        var snapshot = new List<Object_>(objects.Values);
        foreach (var o in snapshot)
        {
            if (o == null) continue;
            if (o is T typed) action(typed);
        }
    }
}