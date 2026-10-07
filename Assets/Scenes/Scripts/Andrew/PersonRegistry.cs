using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Not really using right now...
public static class PersonRegistry
{
    private static readonly Dictionary<string, Person> people = new();

    public static void Register(string personID, Person person)
    {
        if (string.IsNullOrEmpty(personID)) return;
        people[personID] = person;
    }

    public static void Unregister(string personID)
    {
        if (personID != null) people.Remove(personID);
    }

    public static Person Get(string personID)
    {
        return personID != null && people.TryGetValue(personID, out var p) ? p : null;
    }
}