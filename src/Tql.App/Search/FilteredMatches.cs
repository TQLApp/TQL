using System.Collections;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.Search;

internal class FilteredMatches(
    Func<int?, CancellationToken, Task<IEnumerable<IMatch>>> func,
    int? maxResults = null
) : IFilteredAsyncEnumerable
{
    public IFilteredAsyncEnumerable Take(int count)
    {
        return new FilteredMatches(
            func,
            maxResults.HasValue ? Math.Min(count, maxResults.Value) : count
        );
    }

    public Task<IEnumerable<IMatch>> GetValues(CancellationToken cancellationToken) =>
        func(maxResults, cancellationToken);

    public IAsyncEnumerator<IMatch> GetAsyncEnumerator(CancellationToken cancellationToken) =>
        new Enumerator(func(maxResults, cancellationToken));

    public IEnumerator<IMatch> GetEnumerator()
    {
        throw new InvalidOperationException("Filtered matches cannot be enumerated synchronously");
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class Enumerator(Task<IEnumerable<IMatch>> task) : AsyncEnumerator<IMatch>
    {
        protected override async Task<IEnumerable<IMatch>> GetValues() => await task;
    }
}
