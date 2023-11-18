namespace Tql.Abstractions;

/// <summary>
/// Represents a serializable match.
/// </summary>
/// <remarks>
/// <para>
/// TQL uses string serialization to add matches to the history.
/// If you implement this interface on your match class, you indicate
/// that the match can be serialized to text, e.g. JSON. If your
/// match class implements this method, TQL will serialize it whenever
/// the user activates or copies a search results or enters a category.
/// </para>
///
/// <para>
/// When TQL needs to get a match back from the serialized text, it
/// will call <see cref="ITqlPlugin.DeserializeMatch(Guid, string)"/>.
/// The serialized text together with the GUID ID of the match type
/// are passed into this method.
/// </para>
///
/// <para>
/// The assumption is that you'll have a DTO object associated with
/// your match class and that that's serialized to JSON, e.g.
/// using JsonSerializer. The plugin development documentation has
/// examples on how to do this.
/// </para>
/// </remarks>
public interface ISerializableMatch : IMatch
{
    /// <summary>
    /// Serialize a match to text.
    /// </summary>
    /// <returns>Textual representation of the match.</returns>
    string Serialize();
}
