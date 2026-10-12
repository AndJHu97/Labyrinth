using System.Text.RegularExpressions;

public static class TextFormatter
{
    // {word}, {word.property} or {word:argument}
    private static readonly Regex Token = new Regex(@"\{(\w+)(?:\.(\w+))?(?::(\w+))?\}", RegexOptions.Compiled);

    public static string Format(string text, Object_ self = null)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0) return text;
        return Token.Replace(text, m => Resolve(m, self));
    }

    private static string Resolve(Match m, Object_ self)
    {
        string key = m.Groups[1].Value;
        string prop = m.Groups[2].Value.ToLowerInvariant();
        string arg = m.Groups[3].Value;
        bool capitalize = char.IsUpper(key[0]);
        string lowerKey = key.ToLowerInvariant();

        //finds the direction to input before resolving the object_
        if (lowerKey == "direction")
            return ResolveDirection(capitalize);

        Object_ subject;
        switch (lowerKey)
        {
            case "self": subject = self; break;
            case "target": subject = CurrentTarget(); break;
            case "room": subject = GameManager.Instance.currentRoom; break;
            case "name": subject = ObjectRegistry.Get(arg); break;
            default: return m.Value;   // unknown token: leave as written
        }

        

        string value;
        if (subject == null)
            value = lowerKey == "target" ? "something" : null;
        else if (prop == "" || prop == "name")
            value = subject.name_;
        else if (prop == "id")
            value = subject.id;
        else
            return m.Value;                  // unknown property: leave as written

        if (string.IsNullOrEmpty(value)) return m.Value;   // unresolved stays visible
        return capitalize ? char.ToUpperInvariant(value[0]) + value.Substring(1) : value;
    }

    private static Object_ CurrentTarget()
    {
        var log = GameManager.Instance.gameLog;
        if (log.Count == 0) return null;
        return log[log.Count - 1].target;
    }

    private static string ResolveDirection(bool capitalize)
    {
        var gm = GameManager.Instance;

        if (gm == null || gm.gameLog.Count == 0)
            return "{direction}";

        var direction = gm.gameLog[gm.gameLog.Count - 1].concentrationKey;

        string value = direction switch
        {
            ConcentrationKey.Right => "to your right",
            ConcentrationKey.Left => "to your left",
            ConcentrationKey.Up => "above you",
            ConcentrationKey.Down => "below you",
            ConcentrationKey.Straight_Ahead => "in front of you",
            ConcentrationKey.Self => "within yourself",
            ConcentrationKey.Behind => "behind you",
            _ => "{direction}"
        };

        string message = $"There was nothing to interact with {value}.";

        if (capitalize && value.Length > 0)
            value = char.ToUpperInvariant(value[0]) + value.Substring(1);

        return value;
    }

}