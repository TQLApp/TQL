using JetBrains.Annotations;
using Tql.Abstractions;

namespace Tql.Utilities;

/// <summary>
/// Represents a type of match.
/// </summary>
/// <remarks>
/// <para>
/// This interface is used with the <see cref="IMatchTypeManager"/> class.
/// TQL only calls the <see cref="ITqlPlugin.DeserializeMatch(Guid, string)"/>
/// method to deserialize matches. To better structure your code, the
/// <see cref="IMatchTypeManager"/> can be used in conjunction with classes
/// that implement this interface to properly organize match deserialization.
/// </para>
///
/// <para>
/// Better yet, the <see cref="MatchType{TMatch, TDto}"/> base class further
/// improves on this interface by providing a high performance default
/// implementation of all boilerplate required to deserialize matches.
/// Refer to that class, or the plugin documentation, for more information.
/// </para>
/// </remarks>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.Default)]
public interface IMatchType
{
    /// <summary>
    /// Gets the ID of the match type.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Deserialize a match from the serialized text.
    /// </summary>
    /// <param name="value">Serialized match text.</param>
    /// <returns>Deserialized match.</returns>
    IMatch? Deserialize(string value);
}
