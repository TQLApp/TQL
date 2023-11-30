namespace Tql.Abstractions;

/// <summary>
/// Represents a search result.
/// </summary>
public interface IMatch
{
    /// <summary>
    /// Gets the text of the search result to show to the user.
    /// </summary>
    string Text { get; }

    /// <summary>
    /// Gets the icon of the search result.
    /// </summary>
    /// <remarks>
    /// See the ImageFactory class in the utilities library for methods
    /// to load icons, including SVG icons.
    /// </remarks>
    ImageSource Icon { get; }

    /// <summary>
    /// Gets the type ID of the match.
    /// </summary>
    MatchTypeId TypeId { get; }
}
