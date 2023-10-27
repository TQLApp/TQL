﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Services;
using Tql.App.Services.Database;
using Tql.App.Services.Telemetry;
using Tql.App.Support;

namespace Tql.App.Search;

internal class SearchManager : IDisposable
{
    private readonly ILogger<SearchManager> _logger;
    private readonly Settings _settings;
    private readonly IDb _db;
    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly TelemetryService _telemetryService;
    private volatile History? _history;
    private readonly SynchronizationContext _synchronizationContext =
        SynchronizationContext.Current;
    private string _search = string.Empty;
    private int _suspendSearch;
    private readonly Dictionary<string, object> _contextContext = new();
    private int _isSearchingCount;

    public ImmutableArray<SearchResult> Results { get; private set; } =
        ImmutableArray<SearchResult>.Empty;
    public ImmutableArray<ISearchableMatch> Stack { get; private set; } =
        ImmutableArray<ISearchableMatch>.Empty;

    public SearchContext? Context { get; private set; }
    public bool IsSearching => _isSearchingCount > 0;

    public event EventHandler? SearchResultsChanged;
    public event EventHandler? StackChanged;
    public event EventHandler? IsSearchingChanged;

    public SearchManager(
        ILogger<SearchManager> logger,
        Settings settings,
        IDb db,
        IPluginManager pluginManager,
        IServiceProvider serviceProvider,
        TelemetryService telemetryService
    )
    {
        _logger = logger;
        _settings = settings;
        _db = db;
        _pluginManager = pluginManager;
        _serviceProvider = serviceProvider;
        _telemetryService = telemetryService;

        LoadHistory();
    }

    private void LoadHistory()
    {
        // PERFORMANCE: Load the history in the background.

        ThreadPool.QueueUserWorkItem(_ =>
        {
            _logger.LogInformation("Loading history");

            using var telemetry = _telemetryService.CreateDependency(
                "Database",
                "SQLite",
                "LoadHistory"
            );

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
                        IMatch? match;

                        try
                        {
                            match = plugin.DeserializeMatch(entity.TypeId!.Value, entity.Json!);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to deserialize match");
                            continue;
                        }

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

                telemetry.IsSuccess = true;

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
        using (var telemetry = _telemetryService.CreateEvent("Enter Category"))
        {
            match.InitializeTelemetry(telemetry);
        }

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

    public void ReloadHistory()
    {
        LoadHistory();
    }

    public async void DoSearch()
    {
        if (_suspendSearch > 0)
            return;

        _logger.LogInformation("Performing search for '{Search}'", _search);

        using var telemetry = _telemetryService.CreateRequest("Search");

        try
        {
            Context?.Dispose();

            var category = Stack.LastOrDefault();

            category?.InitializeTelemetry(telemetry);

            var context = new SearchContext(_serviceProvider, _search, _history, _contextContext);

            Context = context;

            var results = Stack.Length == 0 ? GetRootSearchResults() : await GetSubSearchResults();

            // If the current _context changed (i.e. isn't our version anymore),
            // a new async task was started while we were working. Don't process
            // the results if this happens.

            if (context != Context)
                return;

            Results = results ?? ImmutableArray<SearchResult>.Empty;

            OnSearchResultsChanged();

            telemetry.AddProperty(nameof(Results), Results.Length.ToString());
            telemetry.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure while performing search");
            _telemetryService.TrackException(ex);
        }
    }

    private ImmutableArray<SearchResult>? GetRootSearchResults()
    {
        var context = Context;
        if (context == null)
            return null;

        // When no search has been entered yet, just return the top
        // 100 items of the history.

        if (context.Search.Length == 0)
        {
            if (context.History == null)
                return null;

            return context.History.Items
                .Select(p => context.GetSearchResult(p.Match))
                .Take(100)
                .ToImmutableArray();
        }

        // Get the root items from all plugins.

        var rootItems = context
            .Filter(_pluginManager.Plugins.SelectMany(p => p.GetMatches()))
            .Select(context.GetSearchResult)
            .Where(p => !p.IsFuzzyMatch)
            .ToList();

        Debug.Assert(
            rootItems.All(p => p.Match is ISerializableMatch),
            "Root items should be serializable"
        );

        if (context.History == null)
        {
            var sorter = new BucketSorter<SearchResult>(2);

            foreach (var item in (IEnumerable<SearchResult>)rootItems)
            {
                sorter.Add(item, item.IsPinned ? 0 : 1);
            }

            return sorter.ToImmutableArray();
        }
        else
        {
            // Filter the history and get the associated states.

            var history = context
                .Filter(context.History.Items.Select(p => p.Match))
                .Select(context.GetSearchResult)
                .ToList();

            // Group fuzzy and non fuzzy matches, and pinned and not pinned.

            var sorter = new BucketSorter<SearchResult>(5);

            foreach (var item in history)
            {
                int bucket;
                if (!item.IsFuzzyMatch)
                    bucket = item.IsPinned ? 0 : 1;
                else
                    bucket = item.IsPinned ? 3 : 4;

                sorter.Add(item, bucket);
            }

            foreach (var item in rootItems)
            {
                if (!item.HistoryId.HasValue)
                    sorter.Add(item, 2);
            }

            return sorter.ToImmutableArray();
        }
    }

    private async Task<ImmutableArray<SearchResult>?> GetSubSearchResults()
    {
        var context = Context;
        if (context == null)
            return null;

        // We let the current match handle filtering. This allows certain
        // matches to fully delegate search to an external service.

        var task = Stack.Last().Search(context, _search, context.CancellationToken);
        var completedInline = task.IsCompleted;

        try
        {
            if (!completedInline)
            {
                _isSearchingCount++;
                OnIsSearchingChanged();

                if (!context.IsPreliminaryResultsSuppressed)
                    ShowPreliminaryResults();
            }

            var result = (await task).Select(context.GetSearchResult).ToList();

            if (result.Count == 0)
            {
                if (context.Search.IsEmpty())
                    return GetPreliminaryResults();
                return ImmutableArray<SearchResult>.Empty;
            }

            // Regardless of how the plugin has ordered the search
            // results, we want the favorited items to appear at the top.

            var ordered = ImmutableArray.CreateBuilder<SearchResult>();

            ordered.AddRange(result.Where(p => p.IsPinned));
            ordered.AddRange(result.Where(p => !p.IsPinned && p.HistoryId.HasValue));
            ordered.AddRange(result.Where(p => !(p.IsPinned || p.HistoryId.HasValue)));

            return ordered.ToImmutable();
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
        var results = GetPreliminaryResults();
        if (results != null)
        {
            Results = results.Value;

            OnSearchResultsChanged();
        }
    }

    private ImmutableArray<SearchResult>? GetPreliminaryResults()
    {
        var context = Context;
        if (context == null || _history == null)
            return null;

        var parent = Stack.Last();
        var parentTypeId = parent.TypeId;
        var parentJson = (parent as ISerializableMatch)?.Serialize();

        var items = _history.Items
            .Where(
                p =>
                    p.History.PluginId == parentTypeId.PluginId
                    && p.History.ParentTypeId == parentTypeId.Id
                    && p.History.ParentJson == parentJson
            )
            .Select(p => p.Match);

        if (!context.Search.IsEmpty())
            items = context.Filter(items);

        return items.Select(context.GetSearchResult).ToImmutableArray();
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

    protected virtual void OnSearchResultsChanged() =>
        SearchResultsChanged?.Invoke(this, EventArgs.Empty);

    protected virtual void OnStackChanged() => StackChanged?.Invoke(this, EventArgs.Empty);

    protected virtual void OnIsSearchingChanged() =>
        IsSearchingChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        Context?.Dispose();
        Context = null;
    }
}
