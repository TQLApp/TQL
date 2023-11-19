using System.Collections;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.Search;

internal partial class SearchContext
{
    private class FilteredMatches : IFilteredAsyncEnumerable
    {
        private readonly SearchContext _owner;
        private readonly ImmutableArray<IMatch> _matches;
        private readonly int? _maxResults;
        private readonly bool _internalCall;

        public FilteredMatches(
            SearchContext owner,
            ImmutableArray<IMatch> matches,
            int? maxResults = null,
            bool internalCall = false
        )
        {
            _owner = owner;
            _matches = matches;
            _maxResults = maxResults;
            _internalCall = internalCall;
        }

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

        private class Enumerator : AsyncEnumerator<IMatch>
        {
            private readonly FilteredMatches _owner;
            private readonly CancellationToken _cancellationToken;

            public Enumerator(FilteredMatches owner, CancellationToken cancellationToken)
            {
                _owner = owner;
                _cancellationToken = cancellationToken;
            }

            protected override async Task<IEnumerable<IMatch>> GetValues()
            {
                return await Task.Run(
                    () =>
                        _owner._owner.DoFilter(
                            _owner._matches,
                            _owner._maxResults,
                            _owner._internalCall
                        ),
                    _cancellationToken
                );
            }
        }
    }
}
