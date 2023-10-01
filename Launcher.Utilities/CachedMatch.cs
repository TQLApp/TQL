using Launcher.Abstractions;

namespace Launcher.Utilities;

public abstract class CachedMatch<T> : ISearchableMatch
{
    private volatile TaskCompletionSource<ImmutableArray<IMatch>> _matches = new();

    public abstract string Text { get; }
    public abstract IImage Icon { get; }
    public abstract Guid TypeId { get; }

    protected CachedMatch(ICache<T> cache)
    {
        cache.Updated += (_, e) => Create(true, e.Cache);

        Task.Run(async () =>
        {
            var data = await cache.Get();

            Create(false, data);
        });
    }

    private void Create(bool update, T data)
    {
        TaskCompletionSource<ImmutableArray<IMatch>> tcs;
        if (update)
            tcs = new();
        else
            tcs = _matches;

        try
        {
            tcs.SetResult(Create(data).ToImmutableArray());
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }
        finally
        {
            if (update)
                _matches = tcs;
        }
    }

    protected abstract IEnumerable<IMatch> Create(T cache);

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        return context.Filter(await _matches.Task);
    }
}
