using System.Windows.Media;
using Tql.Abstractions;

namespace Tql.Utilities;

/// <summary>
/// Provides a base implementation for categories for cached content.
/// </summary>
/// <remarks>
/// This class provides a base implementation for categories that search
/// in cached content. If you've implemented a <see cref="ICacheManager{T}"/>
/// to cache the data you're serving up for a specific category,
/// this class simplifies integrating with the cache.
/// </remarks>
/// <typeparam name="T">Cache data type.</typeparam>
public abstract class CachedMatch<T> : ISearchableMatch
{
    private readonly ICache<T> _cache;
    private ImmutableArray<IMatch>? _matches;

    /// <inheritdoc/>
    public abstract string Text { get; }

    /// <inheritdoc/>
    public abstract ImageSource Icon { get; }

    /// <inheritdoc/>
    public abstract MatchTypeId TypeId { get; }

    /// <inheritdoc/>
    public abstract string SearchHint { get; }

    /// <summary>
    /// Initializes a new <see cref="CachedMatch{T}"/>.
    /// </summary>
    /// <param name="cache">Reference to the cache holding the data.</param>
    protected CachedMatch(ICache<T> cache)
    {
        _cache = cache;

        cache.Updated += (_, e) => _matches = Create(e.Cache).ToImmutableArray();
    }

    /// <summary>
    /// Create matches for all data in the cache.
    /// </summary>
    /// <param name="cache">Cache holding the data.</param>
    /// <returns>Matches for the cached data.</returns>
    protected abstract IEnumerable<IMatch> Create(T cache);

    /// <inheritdoc/>
    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (_matches == null)
            _matches = Create(await _cache.Get()).ToImmutableArray();

        return await context.Filter(_matches);
    }
}
