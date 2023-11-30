using Tql.Abstractions;

namespace Tql.Utilities;

/// <summary>
/// Utility methods for formatting match texts.
/// </summary>
/// <remarks>
/// Use these methods to format the value of the <see cref="IMatch.Text"/>
/// property in a standardized way.
/// </remarks>
public static class MatchText
{
    /// <summary>
    /// Combines multi level match labels in a standardized manner.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Certain match labels refer to hierarchical data. E.g. in Azure
    /// DevOps, repositories are organized in projects. It is preferred
    /// that you include all levels of a hierarchy a match sits in, in
    /// the label of the match.
    /// </para>
    ///
    /// <para>
    /// The reason for this is that this
    /// greatly improves the user experience for searching for these
    /// items in their history. As users use the app more, more often
    /// than not they will find what they need in their history instead
    /// of having to go into categories.  By including all levels of
    /// the hierarchy a match sits in, users can search on any of these
    /// elements to find items in their history.
    /// </para>
    ///
    /// <para>
    /// In case of Azure DevOps, let's say you have an project called
    /// TQL, and a plugin called GitHub, using this method would
    /// format the label for that match as "TQL › GitHub".
    /// </para>
    /// </remarks>
    /// <param name="args">Parts to combine.</param>
    /// <returns>Combined parts.</returns>
    public static string Path(params string?[] args) => string.Join(" › ", args);

    /// <summary>
    /// Combines multi level match labels in a standardized manner.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Certain match labels refer to hierarchical data. E.g. in Azure
    /// DevOps, repositories are organized in projects. It is preferred
    /// that you include all levels of a hierarchy a match sits in, in
    /// the label of the match.
    /// </para>
    ///
    /// <para>
    /// The reason for this is that this
    /// greatly improves the user experience for searching for these
    /// items in their history. As users use the app more, more often
    /// than not they will find what they need in their history instead
    /// of having to go into categories.  By including all levels of
    /// the hierarchy a match sits in, users can search on any of these
    /// elements to find items in their history.
    /// </para>
    ///
    /// <para>
    /// In case of Azure DevOps, let's say you have an project called
    /// TQL, and a plugin called GitHub, using this method would
    /// format the label for that match as "TQL › GitHub".
    /// </para>
    /// </remarks>
    /// <param name="args">Parts to combine.</param>
    /// <param name="options">Options used to combine the parts.</param>
    /// <returns>Combined parts.</returns>
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

    /// <summary>
    /// Formats a match label that is local to a connection.
    /// </summary>
    /// <remarks>
    /// If your plugin allows the user to setup more than one connection,
    /// you would return a copy of the searchable match per connection
    /// (instead of e.g. merging all connections into a single searchable match).
    /// To allow the user to disambiguate between these, the connection
    /// name should be included in the match label. This method is
    /// used to format this in a standardized manner.
    ///
    /// The convention is to only use this format if the user has
    /// defined more than one connection.
    /// </remarks>
    /// <param name="label">Label of the match.</param>
    /// <param name="connectionName">Connection name.</param>
    /// <returns>Formatted match label.</returns>
    public static string ConnectionLabel(string label, string connectionName) =>
        $"{label} ({connectionName})";
}
