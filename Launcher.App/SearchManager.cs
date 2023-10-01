using Launcher.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;
using Launcher.App.Services;
using Launcher.App.Services.Database;

namespace Launcher.App;

internal class SearchManager : IDisposable
{
    private readonly ILogger<SearchManager> _logger;
    private readonly Settings _settings;
    private readonly IDb _db;
    private readonly Task<History> _history;
    private readonly SynchronizationContext _synchronizationContext =
        SynchronizationContext.Current;
    private string _text = string.Empty;

    public ImmutableArray<ISearchableMatch> Stack { get; private set; } =
        ImmutableArray<ISearchableMatch>.Empty;

    public event EventHandler<MatchesEventArgs>? MatchesChanged;
    public event EventHandler? StackChanged;

    public SearchManager(ILogger<SearchManager> logger, Settings settings, IDb db)
    {
        _logger = logger;
        _settings = settings;
        _db = db;

        _history = LoadHistory();
    }

    private Task<History> LoadHistory()
    {
        // PERFORMANCE: Load the history in the background.

        var tcs = new TaskCompletionSource<History>();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            _logger.LogInformation("Loading history");

            try
            {
                using var access = _db.Access();

                var historyEntities = access.GetHistory(
                    _settings.HistoryInRootResults ?? Settings.DefaultHistoryInRootResults
                );

                var history = new History(historyEntities);

                // We let the main thread handle completion of the TCS.
                // This way we can update UI state before we complete the TCS.

                _synchronizationContext.Post(_ => HandleHistoryAvailable(history, tcs), null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load history");

                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    private void HandleHistoryAvailable(History history, TaskCompletionSource<History> tcs)
    {
        tcs.SetResult(history);

        // Only redo the search when we're not yet in a category. Sub item
        // search itself waits for the history to be available.

        if (Stack.Length == 0)
            DoSearch();
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

    public void SearchChanged(string text)
    {
        _text = text;

        DoSearch();
    }

    private void DoSearch()
    {
        if (Stack.Length == 0)
            LoadRootMatches();
        else
            LoadSubMatches();
    }

    private void LoadRootMatches()
    {
        throw new NotImplementedException();
    }

    private void LoadSubMatches()
    {
        throw new NotImplementedException();
    }

    protected virtual void OnMatchesChanged(MatchesEventArgs e) => MatchesChanged?.Invoke(this, e);

    protected virtual void OnStackChanged() => StackChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        // TODO release managed resources here
    }

    private class History
    {
        public History(List<HistoryEntity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
