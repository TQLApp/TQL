using Launcher.Abstractions;
using Launcher.App.Support;
using System.Threading;

namespace Launcher.App;

internal partial class SearchManager
{
    private class SearchContext : ISearchContext
    {
        private Dictionary<IMatch, SearchResult> _state =
            new(ReferenceEqualityComparer<IMatch>.Instance);

        public IServiceProvider ServiceProvider { get; }
        public IDictionary<string, object> Context { get; } = new Dictionary<string, object>();
        public string Search { get; set; }
        public History? History { get; }
        public string SimplifiedSearch { get; }

        public SearchContext(IServiceProvider serviceProvider, string search, History? history)
        {
            ServiceProvider = serviceProvider;
            Search = search;
            History = history;
            SimplifiedSearch = SimplifySearch(search);
        }

        private static string SimplifySearch(string search)
        {
            var sb = StringBuilderCache.Acquire();

            foreach (char c in search)
            {
                if (!char.IsWhiteSpace(c) && !char.IsControl(c))
                    sb.Append(c);
            }

            return TextUtils.RemoveDiacritics(StringBuilderCache.GetStringAndRelease(sb));
        }

        public Task DebounceDelay(CancellationToken cancellationToken) =>
            Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);

        public IEnumerable<IMatch> Filter(IEnumerable<IMatch> matches)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Prefilter(IEnumerable<string> matches)
        {
            throw new NotImplementedException();
        }

        public SearchResult GetMatchState(IMatch match) => _state[match];
    }
}
