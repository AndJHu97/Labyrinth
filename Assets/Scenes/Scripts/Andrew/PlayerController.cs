using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public struct KeyBinding<T> where T : System.Enum
{
    public KeyCode key;
    public T value;
}

[System.Serializable]
public class KeyCategory<T> where T : System.Enum
{
    public List<KeyBinding<T>> bindings = new();

    public bool TryGetValue(KeyCode key, out T value)
    {
        foreach (var b in bindings)
        {
            if (b.key == key)
            {
                value = b.value;
                return true;
            }
        }
        value = default;
        return false;
    }

    // Looks through the typed keys from newest to oldest and takes the
    // most recent one that belongs to this category.
    public bool TryResolve(List<KeyCode> buffer, out T value, out KeyCode usedKey)
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
        usedKey = KeyCode.None;
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

    [Header("Keybinds (only these keys can be typed)")]
    public KeyCategory<EmotionKey> emotion = new();
    public KeyCategory<MotorSensorKey> motor = new();
    public KeyCategory<ConcentrationKey> concentration = new();

    [Header("Console input")]
    public int maxKeys = 10;
    public KeyCode backspaceKey = KeyCode.Backspace;

    // What the player has typed so far (like a console line)
    private readonly List<KeyCode> buffer = new();

    // UI can read this to show the typed line
    public IReadOnlyList<KeyCode> Buffer => buffer;

    void Update()
    {
        if (player == null || player.playerStats.health <= 0) return;

        // Submit with Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Submit();
            return;
        }

        // Delete last typed key
        if (Input.GetKeyDown(backspaceKey))
        {
            if (buffer.Count > 0) buffer.RemoveAt(buffer.Count - 1);
            return;
        }

        // Type a key (only allowed keys, up to the max)
        if (buffer.Count >= maxKeys) return;

        if (TryGetPressedAllowedKey(out KeyCode pressed))
            buffer.Add(pressed);
    }

    // Finds an allowed key that went down this frame
    bool TryGetPressedAllowedKey(out KeyCode pressed)
    {
        pressed = KeyCode.None;

        foreach (var b in emotion.bindings)
            if (Input.GetKeyDown(b.key)) { pressed = b.key; return true; }

        foreach (var b in motor.bindings)
            if (Input.GetKeyDown(b.key)) { pressed = b.key; return true; }

        foreach (var b in concentration.bindings)
            if (Input.GetKeyDown(b.key)) { pressed = b.key; return true; }

        return false;
    }

    void Submit()
    {
        // Nothing typed, nothing happens
        if (buffer.Count == 0) return;

        // Resolve each category: most recent typed key wins, otherwise random
        bool emotionTyped = emotion.TryResolve(buffer, out EmotionKey e, out KeyCode eKey);
        if (!emotionTyped) e = emotion.RandomValue();

        bool motorTyped = motor.TryResolve(buffer, out MotorSensorKey m, out KeyCode mKey);
        if (!motorTyped) m = motor.RandomValue();

        bool concTyped = concentration.TryResolve(buffer, out ConcentrationKey c, out KeyCode cKey);
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

        player.PerformAction(e, m, target);
    }

    void LogSubmission(
        bool emotionTyped, EmotionKey e, KeyCode eKey,
        bool motorTyped, MotorSensorKey m, KeyCode mKey,
        bool concTyped, ConcentrationKey c, KeyCode cKey)
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
}