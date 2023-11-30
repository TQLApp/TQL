namespace Tql.Utilities;

/// <summary>
/// Specifies how match path parts are combined.
/// </summary>
[Flags]
public enum MatchPathOptions
{
    /// <summary>
    /// Identifies no options.
    /// </summary>
    None = 0,

    /// <summary>
    /// Identifies that empty entries should be removed.
    /// </summary>
    RemoveEmptyEntries = 1
}
