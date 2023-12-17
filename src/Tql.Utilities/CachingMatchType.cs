using System.Collections.Concurrent;
using Tql.Abstractions;

namespace Tql.Utilities;

/// <summary>
/// Base class for implementing caching match types.
/// </summary>
/// <remarks>
/// <para>
/// This <see cref="MatchType{TMatch, TDto}"/> sub class caches DTO objects
/// based on a key.
/// </para>
///
/// <para>
/// One use case for this class is to simplify refreshing the status of
/// serialized matches, e.g. the current state of an issue or work item.
/// By default, this class just caches the DTO object based on its key.
/// </para>
///
/// <para>
/// However, you can also call the <see cref="UpdateCache(TDto)"/>
/// and <see cref="UpdateCache(IEnumerable{TDto})"/> methods to overwrite
/// the current entry in the cache. If you do this after you get fresh
/// search results from the server, the next time the DTO object is
/// deserialized, the fresh copy will be used. The app detects this and
/// will automatically update the serialized version in the database.
/// </para>
///
/// <para>
/// If you only need to refresh one or two properties of a DTO
/// object (e.g. the status of an issue or work item), the easiest
/// is to set <typeparamref name="TKey"/> to <typeparamref name="TDto"/>
/// and implement the <c>GetKey</c> method e.g. like this:
/// </para>
///
/// <code><![CDATA[
/// protected override IssueDto GetKey(IssueDto dto)
/// {
///     return dto with { Status = 0 };
/// }
/// ]]></code>
/// </remarks>
/// <typeparam name="TMatch">Type of the match.</typeparam>
/// <typeparam name="TDto">Type of the match DTO object.</typeparam>
/// <typeparam name="TKey">Type of the key to cache to DTO objects with.</typeparam>
/// <param name="factory"></param>
public abstract class CachingMatchType<TMatch, TDto, TKey>(IMatchFactory<TMatch, TDto> factory)
    : MatchType<TMatch, TDto>(factory)
    where TMatch : IMatch
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TDto> _cache = new();

    /// <inheritdoc/>
    protected override TDto DeserializeDto(string value)
    {
        var dto = base.DeserializeDto(value);

        return _cache.GetOrAdd(GetKey(dto), dto);
    }

    /// <summary>
    /// Get the key for the provided DTO object.
    /// </summary>
    /// <param name="dto">The DTO object to get the key for.</param>
    /// <returns>The key for the DTO object.</returns>
    protected abstract TKey GetKey(TDto dto);

    /// <summary>
    /// Update the cache with the provided DTO instances.
    /// </summary>
    /// <param name="dtos">The DTO instances to update the cache with.</param>
    public void UpdateCache(IEnumerable<TDto> dtos)
    {
        foreach (var dto in dtos)
        {
            UpdateCache(dto);
        }
    }

    /// <summary>
    /// Update the cache with the provided DTO instance.
    /// </summary>
    /// <param name="dto">The DTO instance to update the cache with.</param>
    public void UpdateCache(TDto dto)
    {
        _cache[GetKey(dto)] = dto;
    }
}
