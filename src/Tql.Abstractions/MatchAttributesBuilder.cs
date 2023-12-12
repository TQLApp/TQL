namespace Tql.Abstractions;

/// <summary>
/// Builder for <see cref="MatchAttributes"/> instances.
/// </summary>
public class MatchAttributesBuilder
{
    private readonly ImmutableArray<IMatchAttribute>.Builder _builder =
        ImmutableArray.CreateBuilder<IMatchAttribute>();

    internal MatchAttributesBuilder() { }

    /// <summary>
    /// Adds a <see cref="ImageRotationAnimationAttribute"/> attribute to the collection.
    /// </summary>
    /// <param name="duration">Animation duration.</param>
    /// <returns>Itself.</returns>
    public MatchAttributesBuilder WithImageRotationAnimation(TimeSpan duration) =>
        WithAttribute(new ImageRotationAnimationAttribute(duration));

    /// <summary>
    /// Adds the specified attribute to the collection.
    /// </summary>
    /// <param name="attribute">Attribute to add.</param>
    /// <returns>Itself.</returns>
    public MatchAttributesBuilder WithAttribute(IMatchAttribute attribute)
    {
        _builder.Add(attribute);
        return this;
    }

    /// <summary>
    /// Builds the <see cref="MatchAttributes"/>.
    /// </summary>
    /// <returns></returns>
    public MatchAttributes Build() => new(_builder.ToImmutable());
}
