namespace Tql.Utilities;

public static class MatchText
{
    public static string Path(params string?[] args) => string.Join(" › ", args);

    public static string Path(
        IEnumerable<string?> args,
        MatchPathOptions options = MatchPathOptions.None
    )
    {
        var sb = StringBuilderCache.Acquire();
        var removeEmptyEntries = options.HasFlag(MatchPathOptions.RemoveEmptyEntries);

        foreach (var arg in args)
        {
            if (!removeEmptyEntries || !string.IsNullOrEmpty(arg))
            {
                if (sb.Length > 0)
                    sb.Append(" › ");
                sb.Append(arg);
            }
        }

        return StringBuilderCache.GetStringAndRelease(sb);
    }

    public static string ConnectionLabel(string label, string connectionName) =>
        $"{label} ({connectionName})";
}
