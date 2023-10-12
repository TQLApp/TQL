using Launcher.Abstractions;
using System.Windows.Media;

namespace Launcher.Utilities;

public abstract class CachedMatch<T> : ISearchableMatch
{
    private readonly ICache<T> _cache;
    private ImmutableArray<IMatch>? _matches;

    public abstract string Text { get; }
    public abstract ImageSource Icon { get; }
    public abstract MatchTypeId TypeId { get; }

    protected CachedMatch(ICache<T> cache)
    {
        _cache = cache;

        cache.Updated += (_, e) => _matches = Create(e.Cache).ToImmutableArray();
    }

    protected abstract IEnumerable<IMatch> Create(T cache);

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (_matches == null)
            _matches = Create(await _cache.Get()).ToImmutableArray();

        return context.Filter(_matches);
    }
}
