using System.Collections.Concurrent;
using Fastenshtein;
using Launcher.Abstractions;
using Launcher.App.Services.Database;
using Launcher.App.Support;

namespace Launcher.App.Search;

internal class SearchContext : ISearchContext, IDisposable
{
    private readonly ConcurrentDictionary<IMatch, SearchResult> _results =
        new(ReferenceEqualityComparer<IMatch>.Instance);
    private readonly CancellationTokenSource _cts = new();
    private readonly ThreadLocal<Levenshtein> _levenshtein;
    private readonly CharDistribution _distribution;
    private readonly string _lowerSimplifiedSearch;
    private readonly MatchTypeId? _parentTypeId;

    public IServiceProvider ServiceProvider { get; }
    public IDictionary<string, object> Context { get; }
    public string Search { get; set; }
    public History? History { get; }
    public string SimplifiedSearch { get; }
    public CancellationToken CancellationToken => _cts.Token;

    public SearchContext(
        IServiceProvider serviceProvider,
        string search,
        MatchTypeId? parentTypeId,
        History? history,
        IDictionary<string, object> context
    )
    {
        _parentTypeId = parentTypeId;
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

    public IEnumerable<IMatch> Filter(IEnumerable<IMatch> matches)
    {
        var results = new List<SearchResult>();

        Parallel.ForEach(
            matches,
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

        results.Sort(
            (a, b) =>
            {
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
        );

        return results.Select(p => p.Match);
    }

    public IEnumerable<string> Prefilter(IEnumerable<string> matches)
    {
        if (_lowerSimplifiedSearch.Length == 0)
            return matches;

        return matches.Where(IsPartialMatch);
    }

    private bool IsPartialMatch(string match)
    {
        var offset = 0;

        foreach (char c in SimplifyText(match).ToLower())
        {
            if (_lowerSimplifiedSearch[offset] == c)
            {
                offset++;
                if (offset >= _lowerSimplifiedSearch.Length)
                    return true;
            }
        }

        return false;
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

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
