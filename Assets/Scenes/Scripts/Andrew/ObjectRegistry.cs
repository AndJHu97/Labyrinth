using System.Collections.Generic;
using UnityEngine;

public static class ObjectRegistry
{
    private static readonly HashSet<Object_> objects = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry() => objects.Clear();

    public static void Register(Object_ obj) => objects.Add(obj);
    public static void Unregister(Object_ obj) => objects.Remove(obj);

    // First object of a given type, e.g. Get<Door>()
    public static T Get<T>() where T : Object_
    {
        foreach (var o in objects)
            if (o != null && o is T typed) return typed;
        return null;
    }

    // Lookup by display name, for text tokens like {name:Cellar_Door}
    public static Object_ GetByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        foreach (var o in objects)
            if (o != null && string.Equals(o.name_, name, System.StringComparison.OrdinalIgnoreCase))
                return o;
        return null;
    }

    public static void ForEach(System.Action<Object_> action)
    {
        var snapshot = new List<Object_>(objects);
        foreach (var o in snapshot)
            if (o != null) action(o);
    }

    public static void ForEach<T>(System.Action<T> action) where T : Object_
    {
        var snapshot = new List<Object_>(objects);
        foreach (var o in snapshot)
            if (o != null && o is T typed) action(typed);
    }
}