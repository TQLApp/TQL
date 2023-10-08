using System.Diagnostics;
using Launcher.Abstractions;
using Launcher.App.Services;
using Launcher.App.Services.Database;
using Microsoft.Extensions.Logging;

namespace Launcher.App.Search;

internal class SearchManager : IDisposable
{
    private readonly ILogger<SearchManager> _logger;
    private readonly Settings _settings;
    private readonly IDb _db;
    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;
    private volatile History? _history;
    private readonly SynchronizationContext _synchronizationContext =
        SynchronizationContext.Current;
    private string _search = string.Empty;
    private SearchContext? _context;
    private int _suspendSearch;
    private readonly Dictionary<string, object> _contextContext = new();
    private int _isSearchingCount;

    public ImmutableArray<SearchResult> Results { get; private set; } =
        ImmutableArray<SearchResult>.Empty;
    public ImmutableArray<ISearchableMatch> Stack { get; private set; } =
        ImmutableArray<ISearchableMatch>.Empty;

    public bool IsSearching => _isSearchingCount > 0;

    public event EventHandler? SearchResultsChanged;
    public event EventHandler? StackChanged;
    public event EventHandler? IsSearchingChanged;

    public SearchManager(
        ILogger<SearchManager> logger,
        Settings settings,
        IDb db,
        IPluginManager pluginManager,
        IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _settings = settings;
        _db = db;
        _pluginManager = pluginManager;
        _serviceProvider = serviceProvider;

        LoadHistory();
    }

    private void LoadHistory()
    {
        // PERFORMANCE: Load the history in the background.

        ThreadPool.QueueUserWorkItem(_ =>
        {
            _logger.LogInformation("Loading history");

            try
            {
                List<HistoryEntity> historyEntities;

                using (var access = _db.Access())
                {
                    historyEntities = access.GetHistory(
                        _settings.HistoryInRootResults ?? Settings.DefaultHistoryInRootResults
                    );
                }

                var plugins = _pluginManager.Plugins.ToDictionary(p => p.Id, p => p);

                var matches = new List<(HistoryEntity Entity, IMatch Match)>();

                foreach (var entity in historyEntities)
                {
                    if (plugins.TryGetValue(entity.PluginId!.Value, out var plugin))
                    {
                        var match = plugin.DeserializeMatch(entity.TypeId!.Value, entity.Json!);
                        if (match != null)
                        {
                            var json = ((ISerializableMatch)match).Serialize();
                            if (json != entity.Json)
                            {
                                _logger.LogWarning(
                                    "Serialized JSON did not match the JSON stored in the history table"
                                );

                                entity.Json = json;
                            }

                            matches.Add((entity, match));
                        }
                    }
                }

                _history = new History(matches);

                _synchronizationContext.Post(_ => DoSearch(), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load history");
            }
        });
    }

    public void Push(ISearchableMatch match)
    {
        Stack = Stack.Add(match);

        ClearResults();

        OnStackChanged();

        DoSearch();
    }

    public void Pop()
    {
        if (Stack.Length == 0)
            return;

        ClearResults();

        Stack = Stack.RemoveAt(Stack.Length - 1);

        OnStackChanged();

        DoSearch();
    }

    private void ClearResults()
    {
        Results = ImmutableArray<SearchResult>.Empty;

        OnSearchResultsChanged();
    }

    public void SearchChanged(string search)
    {
        _search = search;

        DoSearch();
    }

    public async void DoSearch()
    {
        if (_suspendSearch > 0)
            return;

        _logger.LogInformation("Performing search for '{Search}'", _search);

        try
        {
            _context?.Dispose();

            var context = new SearchContext(
                _serviceProvider,
                _search,
                Stack.LastOrDefault()?.TypeId,
                _history,
                _contextContext
            );

            _context = context;

            var results = Stack.Length == 0 ? GetRootSearchResults() : await GetSubSearchResults();

            // If the current _context changed (i.e. isn't our version anymore),
            // a new async task was started while we were working. Don't process
            // the results if this happens.

            if (context != _context)
                return;

            Results = results?.ToImmutableArray() ?? ImmutableArray<SearchResult>.Empty;

            OnSearchResultsChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure while performing search");
        }
    }

    private IEnumerable<SearchResult>? GetRootSearchResults()
    {
        var context = _context;
        if (context == null)
            return null;

        // When no search has been entered yet, just return the top
        // 100 items of the history.

        if (context.Search.Length == 0)
            return context.History?.Items.Select(p => context.GetSearchResult(p.Match)).Take(100);

        // Get the root items from all plugins.

        var rootItems = context
            .Filter(_pluginManager.Plugins.SelectMany(p => p.GetMatches()))
            .Select(context.GetSearchResult)
            .Where(p => p.Penalty <= 0)
            .ToList();

        Debug.Assert(
            rootItems.All(p => p.Match is ISerializableMatch),
            "Root items should be serializable"
        );

        if (context.History == null)
            return rootItems;

        // Filter the history and get the associated states.

        var history = context
            .Filter(context.History.Items.Select(p => p.Match))
            .Select(context.GetSearchResult)
            .ToList();

        // Sort exact matches of the history above the root items, and
        // fuzzy matches below them.

        var result = new List<SearchResult>();

        result.AddRange(history.Where(p => p.Penalty < 0));
        result.AddRange(rootItems);
        result.AddRange(history.Where(p => p.Penalty >= 0));

        return result;
    }

    private async Task<IEnumerable<SearchResult>?> GetSubSearchResults()
    {
        var context = _context;
        if (context == null)
            return null;

        // We let the current match handle filtering. This allows certain
        // matches to fully delegate search to an external service.

        var task = Stack.Last().Search(context, _search, context.CancellationToken);
        var completedInline = task.IsCompleted;

        if (!completedInline)
        {
            _isSearchingCount++;
            OnIsSearchingChanged();

            if (!context.IsPreliminaryResultsSuppressed)
                ShowPreliminaryResults();
        }

        try
        {
            var result = await task;

            return result.Select(context.GetSearchResult);
        }
        finally
        {
            if (!completedInline)
            {
                _isSearchingCount--;
                OnIsSearchingChanged();
            }
        }
    }

    private void ShowPreliminaryResults()
    {
        if (_context == null || _history == null)
            return;

        var parentTypeId = Stack.Last().TypeId;

        Results = _context
            .Filter(
                _history.Items
                    .Where(
                        p =>
                            p.History.PluginId == parentTypeId.PluginId
                            && p.History.ParentTypeId == parentTypeId.Id
                    )
                    .Select(p => p.Match)
            )
            .Select(_context.GetSearchResult)
            .ToImmutableArray();

        OnSearchResultsChanged();
    }

    public void SuspendSearch()
    {
        _suspendSearch++;
    }

    public void ResumeSearch(bool performSearch = true)
    {
        if (_suspendSearch == 0)
            throw new InvalidOperationException("Unbalanced SuspendSearch and ResumeSearch");

        _suspendSearch--;

        if (_suspendSearch == 0 && performSearch)
            DoSearch();
    }

    public void DeleteHistory(long historyId)
    {
        _history?.Items.RemoveAll(p => p.History.Id == historyId);

        DoSearch();
    }

    protected virtual void OnSearchResultsChanged() =>
        SearchResultsChanged?.Invoke(this, EventArgs.Empty);

    protected virtual void OnStackChanged() => StackChanged?.Invoke(this, EventArgs.Empty);

    protected virtual void OnIsSearchingChanged() =>
        IsSearchingChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _context?.Dispose();
        _context = null;
    }
}
