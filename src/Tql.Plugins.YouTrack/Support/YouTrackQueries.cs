using System.Text.RegularExpressions;

namespace Tql.Plugins.YouTrack.Support;

// YouTrack search queries the plugin builds from what the user types. The text
// goes to YouTrack as a query, so "#Unresolved for: me" works. YouTrack
// matches word prefixes on its own ("kubecod" finds "KubeCoder"), so nothing
// appends a "*", which could also break a query the user typed.
//
// This file has no WPF or TQL dependencies, so the plain net8.0 test project
// can compile it.
internal static class YouTrackQueries
{
    // A project short name starts with a letter. Matching ignores case:
    // YouTrack finds "kc-230" as well as "KC-230".
    private static readonly Regex IssueIdRe =
        new(@"^[A-Z][A-Z0-9_]*-\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PlainValueRe = new(@"^\w+$", RegexOptions.Compiled);

    /// <summary>
    /// Whether the text is an issue ID, such as "KC-230", which the plugin
    /// fetches directly before it searches.
    /// </summary>
    public static bool IsIssueId(string text) => IssueIdRe.IsMatch(text.Trim());

    /// <summary>
    /// The issues of a project: "project: KC text", or the most recently
    /// updated ones without text.
    /// </summary>
    public static string Project(string shortName, string? text)
    {
        var query = $"project: {Value(shortName)}";

        if (string.IsNullOrWhiteSpace(text))
            return query + " sort by: updated desc";

        return $"{query} {text.Trim()}";
    }

    /// <summary>
    /// A saved search narrowed with the text: "(saved query) and (text)".
    /// YouTrack accepts a "sort by:" inside the parentheses.
    /// </summary>
    public static string SavedSearch(string savedQuery, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return savedQuery;
        if (string.IsNullOrWhiteSpace(savedQuery))
            return text.Trim();

        return $"({savedQuery.Trim()}) and ({text.Trim()})";
    }

    /// <summary>
    /// A value as the right-hand side of an attribute, in braces when it is
    /// more than letters, digits and underscores.
    /// </summary>
    public static string Value(string value) =>
        PlainValueRe.IsMatch(value) ? value : $"{{{value}}}";
}
