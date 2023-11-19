using System.Collections.Concurrent;
using Tql.Abstractions;
using Tql.App.Services.Database;
using Tql.App.Support;

namespace Tql.App.Search;

internal partial class SearchContext : ISearchContext, IDisposable
{
    private readonly ConcurrentDictionary<IMatch, SearchResult> _results =
        new(ReferenceEqualityComparer<IMatch>.Instance);
    private readonly CancellationTokenSource _cts = new();
    private readonly ThreadLocal<Levenshtein> _levenshtein;
    private readonly CharDistribution _distribution;
    private readonly string _lowerSimplifiedSearch;

    public IServiceProvider ServiceProvider { get; }
    public IDictionary<string, object> Context { get; }
    public string Search { get; set; }
    public History? History { get; }
    public string SimplifiedSearch { get; }
    public CancellationToken CancellationToken => _cts.Token;
    public bool IsPreliminaryResultsSuppressed { get; private set; }
    public bool IsFiltered { get; private set; }

    public SearchContext(
        IServiceProvider serviceProvider,
        string search,
        History? history,
        IDictionary<string, object> context
    )
    {
        ServiceProvider = serviceProvider;
        Search = search;
        History = history;
        SimplifiedSearch = SimplifyText(search);
        Context = context;

        _lowerSimplifiedSearch = SimplifiedSearch.ToLower();
        _levenshtein = new ThreadLocal<Levenshtein>(() => new Levenshtein(_lowerSimplifiedSearch));
        _distribution = new CharDistribution(_lowerSimplifiedSearch);
    }

    private static string SimplifyText(string search)
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

    public IFilteredAsyncEnumerable Filter(IEnumerable<IMatch> matches) =>
        new FilteredMatches(this, matches.ToImmutableArray());

    public IFilteredAsyncEnumerable FilterInternal(IEnumerable<IMatch> matches) =>
        new FilteredMatches(this, matches.ToImmutableArray(), internalCall: true);

    private IEnumerable<IMatch> DoFilter(
        IEnumerable<IMatch> matches,
        int? maxResults,
        bool internalCall
    )
    {
        if (!internalCall)
            IsFiltered = true;

        var results = new List<SearchResult>();

        var options = new ParallelOptions { CancellationToken = CancellationToken };

        Parallel.ForEach(
            matches,
            options,
            match =>
            {
                var searchResult = GetSearchResult(match);

                if (searchResult.Penalty is <= Constants.MaxPenalty)
                {
                    lock (results)
                    {
                        results.Add(searchResult);
                    }
                }
            }
        );

        CancellationToken.ThrowIfCancellationRequested();

        results.Sort(new SearchResultComparer(CancellationToken));

        if (maxResults.HasValue)
            return results.Take(maxResults.Value).Select(p => p.Match);

        return results.Select(p => p.Match);
    }

    public SearchResult GetSearchResult(IMatch match)
    {
        return _results.GetOrAdd(match, CreateSearchResult);
    }

    private SearchResult CreateSearchResult(IMatch match)
    {
        var text = match.Text;
        var simpleText = TextUtils.RemoveDiacritics(match.Text);
        var lowerSimpleText = simpleText.ToLower();
        var json = (match as ISerializableMatch)?.Serialize();
        var history = default(HistoryEntity);
        var penalty = default(int?);

        if (json != null)
            history = History?.GetByJson(match.TypeId, json)?.History;

        var textMatch = TextMatching.Match(_lowerSimplifiedSearch, lowerSimpleText, _distribution);

        // Distance of -1 signifies an exact match.

        var distance = -1;

        var fuzzyText = default(string);

        if (textMatch == null)
        {
            (fuzzyText, int fuzzyTextDistance) = FuzzyTextMatching.Match(
                _levenshtein.Value,
                _lowerSimplifiedSearch,
                _distribution,
                simpleText,
                lowerSimpleText
            );

            if (fuzzyText != null)
            {
                var fuzzyMatches = TextMatching.Find(fuzzyText.ToLower(), lowerSimpleText);

                textMatch = new TextMatch(fuzzyMatches!.ToImmutableArray());

                distance = fuzzyTextDistance;
            }
        }

        if (textMatch != null)
            penalty = CalculatePenalty(textMatch, simpleText, history, distance);

        return new SearchResult(
            match,
            text,
            simpleText,
            fuzzyText != null,
            textMatch,
            history?.Id,
            history?.LastAccess,
            (history?.IsPinned ?? 0) != 0,
            penalty
        );
    }

    private int CalculatePenalty(
        TextMatch? textMatch,
        string simpleText,
        HistoryEntity? history,
        int? distance
    )
    {
        // Add 1 to make distance start at 0.
        int distancePenalty = distance.HasValue ? distance.Value + 1 : 0;

        // Add item match count, starting at 0, except for zero matches which gets a penalty of 1.
        int itemMatchCountPenalty =
            textMatch == null || textMatch.Ranges.Length == 0 ? 1 : textMatch.Ranges.Length - 1;

        // Add 1 if the first match doesn't start at a word boundary.
        int wordBoundaryPenalty =
            textMatch == null || textMatch.Ranges.Length == 0
                ? 1
                : (TextUtils.IsWordBoundary(simpleText, textMatch.Ranges[0].Offset) ? 0 : 1);

        // Add a penalty based on how often the favorite was accessed.
        int accessCountPenalty =
            history != null
                ? (
                    history.AccessCount!.Value < 5
                        ? -1
                        : history.AccessCount!.Value < 10
                            ? -2
                            : -3
                )
                : 0;

        return distancePenalty + itemMatchCountPenalty + wordBoundaryPenalty + accessCountPenalty;
    }

    public void SuppressPreliminaryResults()
    {
        IsPreliminaryResultsSuppressed = true;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private class SearchResultComparer : IComparer<SearchResult>
    {
        private readonly CancellationToken _cancellationToken;

        public SearchResultComparer(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public int Compare(SearchResult a, SearchResult b)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            int result = a.Penalty!.Value.CompareTo(b.Penalty!.Value);
            if (result != 0)
                return result;

            if (a.TextMatch?.Ranges.Length > 0 && b.TextMatch?.Ranges.Length > 0)
            {
                result = a.TextMatch!.Ranges[0].Offset.CompareTo(b.TextMatch!.Ranges[0].Offset);
                if (result != 0)
                    return result;
            }

            result = string.Compare(
                a.SimpleText,
                b.SimpleText,
                StringComparison.CurrentCultureIgnoreCase
            );
            if (result != 0)
                return result;

            return -a.LastAccessed
                .GetValueOrDefault()
                .CompareTo(b.LastAccessed.GetValueOrDefault());
        }
    }
}
