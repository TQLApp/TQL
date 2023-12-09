namespace Tql.Abstractions;

/// <summary>
/// Represents a match that has attributes.
/// </summary>
public interface IHasMatchAttributes : IMatch
{
    /// <summary>
    /// Gets the attributes of the match.
    /// </summary>
    MatchAttributes Attributes { get; }
}
