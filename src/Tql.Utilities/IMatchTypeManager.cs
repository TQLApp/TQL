using Tql.Abstractions;

namespace Tql.Utilities;

/// <summary>
/// Represents a match type manager.
/// </summary>
/// <remarks>
/// <para>
/// The match type manager is used to manage match types for your
/// plugin and route deserialization request to the applicable
/// match types.
/// </para>
///
/// <para>
/// The way to use this service is to build one using <see cref="MatchTypeManagerBuilder"/>
/// and call the <see cref="Deserialize(Guid, string)"/> method
/// from your <see cref="ITqlPlugin.DeserializeMatch(Guid, string)"/>
/// implementation.
/// </para>
/// </remarks>
public interface IMatchTypeManager
{
    /// <summary>
    /// Gets all match types.
    /// </summary>
    ImmutableArray<IMatchType> MatchTypes { get; }

    /// <summary>
    /// Deserializes a match if a match type is registered for
    /// the specified type ID.
    /// </summary>
    /// <param name="typeId">Match type ID.</param>
    /// <param name="value">Serialized match text.</param>
    /// <returns>Deserialized match.</returns>
    IMatch? Deserialize(Guid typeId, string value);
}
