using System.Diagnostics.CodeAnalysis;
using Tql.Abstractions;

namespace Tql.Utilities;

/// <summary>
/// Base class for implementing match types.
/// </summary>
/// <remarks>
/// <para>
/// This base class provides boilerplate for implementing match types
/// (see <see cref="IMatchType"/>) following best practices.
/// </para>
///
/// <para>
/// This implementation uses <see cref="IMatchFactory{TMatch, TDto}"/>,
/// which assumes that you're using DI for your matches, and that
/// you have a single DTO object for your matches. This class takes
/// care of deserialization and instantiation for you.
/// </para>
///
/// <para>
/// You should implement the <see cref="IsValid(TDto)"/> method.
/// The implementation of this method should validate that the
/// match DTO object is still valid. If your plugin e.g. allows the
/// user to define connections, this method would check whether the
/// connection still exists.
/// </para>
/// </remarks>
/// <typeparam name="TMatch">Type of the match.</typeparam>
/// <typeparam name="TDto">Type of the match DTO object.</typeparam>
public abstract class MatchType<TMatch, TDto> : IMatchType
    where TMatch : IMatch
{
    private readonly IMatchFactory<TMatch, TDto> _factory;

    /// <inheritdoc/>
    public abstract Guid Id { get; }

    /// <summary>
    /// Instantiates a new <see cref="MatchType{TMatch, TDto}"/>.
    /// </summary>
    /// <param name="factory">Match factory.</param>
    protected MatchType(IMatchFactory<TMatch, TDto> factory)
    {
        _factory = factory;
    }

    /// <inheritdoc/>
    public IMatch? Deserialize(string value)
    {
        var dto = DeserializeDto(value);
        if (!IsValid(dto))
            return null;

        return _factory.Create(dto);
    }

    /// <summary>
    /// Deserializes the JSON value into a DTO object.
    /// </summary>
    /// <remarks>
    /// Override this if you need to make changes to the returned DTO object.
    /// </remarks>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual TDto DeserializeDto(string value)
    {
        return JsonSerializer.Deserialize<TDto>(value)!;
    }

    /// <summary>
    /// Checks whether the match DTO object is still valid.
    /// </summary>
    /// <param name="dto">DTO object to validate.</param>
    /// <returns>Whether the DTO object is valid.</returns>
    protected virtual bool IsValid(TDto dto) => true;
}
