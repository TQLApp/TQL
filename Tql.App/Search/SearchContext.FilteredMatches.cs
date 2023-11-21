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
        private readonly SearchContext _owner = owner;
        private readonly ImmutableArray<IMatch> _matches = matches;
        private readonly int? _maxResults = maxResults;
        private readonly bool _internalCall = internalCall;

        public IFilteredAsyncEnumerable Take(int count)
        {
            return new FilteredMatches(
                _owner,
                _matches,
                _maxResults.HasValue ? Math.Min(count, _maxResults.Value) : count,
                _internalCall
            );
        }

        public IAsyncEnumerator<IMatch> GetAsyncEnumerator(CancellationToken cancellationToken) =>
            new Enumerator(this, cancellationToken);

        public IEnumerator<IMatch> GetEnumerator()
        {
            throw new InvalidOperationException(
                "Filtered matches cannot be enumerated synchronously"
            );
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator(FilteredMatches owner, CancellationToken cancellationToken)
            : AsyncEnumerator<IMatch>
        {
            protected override async Task<IEnumerable<IMatch>> GetValues()
            {
                return await Task.Run(
                    () =>
                        owner
                            ._owner
                            .DoFilter(owner._matches, owner._maxResults, owner._internalCall),
                    cancellationToken
                );
            }
        }
    }
}
