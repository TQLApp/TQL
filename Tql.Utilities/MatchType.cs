using System.Text.Json;
using Tql.Abstractions;

namespace Tql.Utilities;

/// <summary>
/// Base class for implementing match types.
/// </summary>
/// <remarks>
/// This base class provides boiler plate for implementing match types
/// (see <see cref="IMatchType"/>) following best practices.
///
/// This implementation uses <see cref="IMatchFactory{TMatch, TDto}"/>,
/// which assumes that you're using DI for your matches, and that
/// you have a single DTO object for your matches. This class takes
/// care of deserialization and instantiation for you.
///
/// You should implement the <see cref="IsValid(TDto)"/> method.
/// The implementation of this method should validate that the
/// match DTO object is still valid. If your plugin e.g. allows the
/// user to define connections, this method would check whether the
/// connection still exists.
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
        var dto = JsonSerializer.Deserialize<TDto>(value)!;
        if (!IsValid(dto))
            return null;

        return _factory.Create(dto);
    }

    /// <summary>
    /// Checks whether the match DTO object is still valid.
    /// </summary>
    /// <param name="dto">DTO object to validate.</param>
    /// <returns>Whether the DTO object is valid.</returns>
    protected virtual bool IsValid(TDto dto) => true;
}
