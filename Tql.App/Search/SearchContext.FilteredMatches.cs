using System.Collections;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.Search;

internal partial class SearchContext
{
    private class FilteredMatches(
        SearchContext owner,
        ImmutableArray<IMatch> matches,
        int? maxResults = null,
        bool internalCall = false
    ) : IFilteredAsyncEnumerable
    {
        public IFilteredAsyncEnumerable Take(int count)
        {
            return new FilteredMatches(
                owner,
                matches,
                maxResults.HasValue ? Math.Min(count, maxResults.Value) : count,
                internalCall
            );
        }

        public IAsyncEnumerator<IMatch> GetAsyncEnumerator(CancellationToken cancellationToken) =>
            new Enumerator(owner, matches, maxResults, internalCall, cancellationToken);

        public IEnumerator<IMatch> GetEnumerator()
        {
            throw new InvalidOperationException(
                "Filtered matches cannot be enumerated synchronously"
            );
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator(
            SearchContext owner,
            ImmutableArray<IMatch> matches,
            int? maxResults,
            bool internalCall,
            CancellationToken cancellationToken
        ) : AsyncEnumerator<IMatch>
        {
            protected override async Task<IEnumerable<IMatch>> GetValues()
            {
                return await Task.Run(
                    () => owner.DoFilter(matches, maxResults, internalCall),
                    cancellationToken
                );
            }
        }
    }
}
