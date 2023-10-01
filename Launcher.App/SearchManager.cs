using Launcher.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;
using Launcher.App.Services;
using Launcher.App.Services.Database;

namespace Launcher.App;

internal partial class SearchManager : IDisposable
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

    public ImmutableArray<SearchResult> Results { get; private set; } =
        ImmutableArray<SearchResult>.Empty;
    public ImmutableArray<ISearchableMatch> Stack { get; private set; } =
        ImmutableArray<ISearchableMatch>.Empty;

    public event EventHandler? SearchResultsChanged;
    public event EventHandler? StackChanged;

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
                            matches.Add((entity, match));
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

        OnStackChanged();

        DoSearch();
    }

    public void Pop()
    {
        if (Stack.Length == 0)
            return;

        Stack = Stack.RemoveAt(Stack.Length - 1);

        OnStackChanged();

        DoSearch();
    }

    public void SearchChanged(string search)
    {
        _search = search;

        DoSearch();
    }

    private void DoSearch()
    {
        _context = new SearchContext(_serviceProvider, _search, _history);

        var results = Stack.Length == 0 ? GetRootSearchResults() : GetSubSearchResults();

        if (results != null)
            Results = results.ToImmutableArray();
        else
            Results = ImmutableArray<SearchResult>.Empty;

        OnSearchResultsChanged();
    }

    private IEnumerable<SearchResult>? GetRootSearchResults()
    {
        if (_context == null)
            return null;

        // When no search has been entered yet, just return the top
        // 100 items of the history.

        if (_context.Search.Length == 0)
            return _context.History?.Items.Select(p => _context.GetMatchState(p.Match)).Take(100);

        // Get the root items from all plugins.

        var rootItems = _context
            .Filter(_pluginManager.Plugins.SelectMany(p => p.GetMatches()))
            .Select(_context.GetMatchState)
            .Where(p => p.Penalty <= 0)
            .ToList();

        if (_context.History == null)
            return rootItems;

        // Filter the history and get the associated states.

        var history = _context
            .Filter(_context.History.Items.Select(p => p.Match))
            .Select(_context.GetMatchState)
            .ToList();

        // Sort exact matches of the history above the root items, and
        // fuzzy matches below them.

        var result = new List<SearchResult>();

        result.AddRange(history.Where(p => p.Penalty < 0));
        result.AddRange(rootItems);
        result.AddRange(history.Where(p => p.Penalty >= 0));

        return result;
    }

    private IEnumerable<SearchResult>? GetSubSearchResults()
    {
        throw new NotImplementedException();
    }

    protected virtual void OnSearchResultsChanged() =>
        SearchResultsChanged?.Invoke(this, EventArgs.Empty);

    protected virtual void OnStackChanged() => StackChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        // TODO release managed resources here
    }

    private class History
    {
        public List<(HistoryEntity History, IMatch Match)> Items { get; }

        public History(IEnumerable<(HistoryEntity History, IMatch Match)> items)
        {
            Items = items.ToList();
        }
    }
}
