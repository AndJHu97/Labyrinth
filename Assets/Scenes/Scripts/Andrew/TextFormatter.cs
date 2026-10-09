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

}