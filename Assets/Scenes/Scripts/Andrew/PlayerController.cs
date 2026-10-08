using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public struct KeyBinding<T> where T : System.Enum
{
    [Tooltip("A single character, e.g. q. Upper or lower case doesn't matter.")]
    public string key;
    public T value;

    // The bound character, always lower case ('\0' if the field is empty)
    public char Char => string.IsNullOrEmpty(key) ? '\0' : char.ToLowerInvariant(key[0]);
}

[System.Serializable]
public class KeyCategory<T> where T : System.Enum
{
    public List<KeyBinding<T>> bindings = new();

    // Is this character bound in this category?
    public bool Contains(char c)
    {
        foreach (var b in bindings)
            if (b.Char == c) return true;
        return false;
    }

    public bool TryGetValue(char c, out T value)
    {
        foreach (var b in bindings)
        {
            if (b.Char == c)
            {
                value = b.value;
                return true;
            }
        }
        value = default;
        return false;
    }

    // Looks through the typed characters from newest to oldest and takes the
    // most recent one that belongs to this category.
    public bool TryResolve(List<char> buffer, out T value, out char usedKey)
    {
        for (int i = buffer.Count - 1; i >= 0; i--)
        {
            if (TryGetValue(buffer[i], out value))
            {
                usedKey = buffer[i];
                return true;
            }
        }
        value = default;
        usedKey = '\0';
        return false;
    }

    public T RandomValue()
    {
        var values = (T[])System.Enum.GetValues(typeof(T));
        return values[Random.Range(0, values.Length)];
    }
}

public class PlayerController : MonoBehaviour
{
    public Player player;

    [Header("Keybinds (type these characters; case is ignored)")]
    public KeyCategory<EmotionKey> emotion = new();
    public KeyCategory<MotorSensorKey> motor = new();
    public KeyCategory<ConcentrationKey> concentration = new();

    [Header("Console input")]
    public int maxKeys = 3;

    // What the player has typed so far (like a console line), always lower case
    private readonly List<char> buffer = new();

    // UI can read this to show the typed line
    public IReadOnlyList<char> Buffer => buffer;

    void Awake()
    {
        ValidateBindings();
    }

    void Update()
    {
        if (player == null || player.playerStats.health <= 0) return;

        // inputString holds the characters typed this frame
        foreach (char raw in Input.inputString)
        {
            // Enter submits
            if (raw == '\n' || raw == '\r')
            {
                Submit();
                return;
            }

            // Backspace deletes the last typed character
            if (raw == '\b')
            {
                if (buffer.Count > 0) buffer.RemoveAt(buffer.Count - 1);
                continue;
            }

            // Lower-case automatically
            char c = char.ToLowerInvariant(raw);

            // Only allowed characters, up to the max
            if (buffer.Count >= maxKeys) continue;
            if (IsAllowed(c)) buffer.Add(c);
        }
    }

    bool IsAllowed(char c)
    {
        return emotion.Contains(c) || motor.Contains(c) || concentration.Contains(c);
    }

    void Submit()
    {
        // Nothing typed, nothing happens
        if (buffer.Count == 0) return;

        // Resolve each category: most recent typed character wins, otherwise random
        bool emotionTyped = emotion.TryResolve(buffer, out EmotionKey e, out char eKey);
        if (!emotionTyped) e = emotion.RandomValue();

        bool motorTyped = motor.TryResolve(buffer, out MotorSensorKey m, out char mKey);
        if (!motorTyped) m = motor.RandomValue();

        bool concTyped = concentration.TryResolve(buffer, out ConcentrationKey c, out char cKey);
        if (!concTyped) c = concentration.RandomValue();

        LogSubmission(emotionTyped, e, eKey, motorTyped, m, mKey, concTyped, c, cKey);

        buffer.Clear();

        // Self-interaction is its own method (TODO in Player)
        if (c == ConcentrationKey.Self)
        {
            player.PerformAction(e, m);
            return;
        }

        var room = GameManager.Instance.currentRoom;
        Object_ target = room != null ? room.GetPart(c) : null;

        player.PerformAction(e, m, c, target);
    }

    void LogSubmission(
        bool emotionTyped, EmotionKey e, char eKey,
        bool motorTyped, MotorSensorKey m, char mKey,
        bool concTyped, ConcentrationKey c, char cKey)
    {
        var sb = new StringBuilder();

        sb.Append("Submitted keys: [");
        for (int i = 0; i < buffer.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(buffer[i]);
        }
        sb.AppendLine("]");

        sb.AppendLine($"  Emotion:       {e} {(emotionTyped ? $"(typed {eKey})" : "(random)")}");
        sb.AppendLine($"  Motor/Sensor:  {m} {(motorTyped ? $"(typed {mKey})" : "(random)")}");
        sb.Append($"  Concentration: {c} {(concTyped ? $"(typed {cKey})" : "(random)")}");

        Debug.Log(sb.ToString());
    }

    // Warns about bad bindings so they can be fixed in the inspector
    void ValidateBindings()
    {
        foreach (var b in emotion.bindings) CheckBinding(b.key, b.Char, "Emotion");
        foreach (var b in motor.bindings) CheckBinding(b.key, b.Char, "Motor");
        foreach (var b in concentration.bindings) CheckBinding(b.key, b.Char, "Concentration");

        foreach (var b in emotion.bindings)
            if (motor.Contains(b.Char) || concentration.Contains(b.Char))
                Debug.LogWarning($"PlayerController: '{b.Char}' is bound in more than one category.");

        foreach (var b in motor.bindings)
            if (concentration.Contains(b.Char))
                Debug.LogWarning($"PlayerController: '{b.Char}' is bound in more than one category.");
    }

    void CheckBinding(string raw, char c, string category)
    {
        if (c == '\0')
            Debug.LogWarning($"PlayerController: a {category} binding has an empty key.");
        else if (raw.Length > 1)
            Debug.LogWarning($"PlayerController: {category} binding '{raw}' has more than one character; only '{c}' is used.");
        else if (c == '\n' || c == '\r' || c == '\b')
            Debug.LogWarning($"PlayerController: {category} binding uses a reserved character (Enter/Backspace).");
    }
}