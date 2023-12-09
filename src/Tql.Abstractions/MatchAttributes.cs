namespace Tql.Abstractions;

/// <summary>
/// Holds attributes of a match.
/// </summary>
/// <remarks>
/// <para>
/// Match attributes are a collection of attributes to tag on metadata
/// to a match. The purpose of this mechanism is to allow you to expose
/// attributes in a dynamic manner, without having to bloat the match
/// itself.
/// </para>
///
/// <para>
/// The advised usage pattern is to use the <see cref="CreateBuilder()"/>
/// method to build a <see cref="MatchAttributes"/> instance that you
/// assign to a static field. You then reuse that instance for every
/// match or variation of a match the attributes apply to.
/// </para>
/// </remarks>
/// <param name="Attributes"></param>
public record MatchAttributes(ImmutableArray<IMatchAttribute> Attributes)
{
    /// <summary>
    /// Match attributes instance without any attributes.
    /// </summary>
    public static MatchAttributes Empty = new(ImmutableArray<IMatchAttribute>.Empty);

    /// <summary>
    /// Creates a new <see cref="MatchAttributesBuilder"/>.
    /// </summary>
    /// <returns></returns>
    public static MatchAttributesBuilder CreateBuilder() => new();

    /// <summary>
    /// Finds an attribute of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the attribute to find.</typeparam>
    /// <returns>First attribute of the specified type, or <c>null</c> of no such attribute exists.</returns>
    public T? GetAttribute<T>()
        where T : IMatchAttribute
    {
        foreach (var attribute in Attributes)
        {
            if (attribute is T match)
                return match;
        }

        return default;
    }
}

/// <summary>
/// Represents a match attribute.
/// </summary>
public interface IMatchAttribute;

/// <summary>
/// Specifies that the image of a match needs to rotate
/// around its center for the specified duration.
/// </summary>
/// <param name="Duration">Animation duration.</param>
public record ImageRotationAnimationAttribute(TimeSpan Duration) : IMatchAttribute;
