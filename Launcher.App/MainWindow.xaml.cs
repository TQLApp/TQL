using Launcher.Abstractions;
using Launcher.App.Interop;
using Launcher.App.Search;
using Launcher.App.Services;
using Launcher.App.Services.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Launcher.App;

internal partial class MainWindow
{
    private const int ResultItemsCount = 8;

    private readonly Settings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindow> _logger;
    private readonly IDb _db;
    private readonly CacheManagerManager _cacheManagerManager;
    private KeyboardHook? _keyboardHook;
    private SearchManager? _searchManager;
    private readonly UI _ui;
    private double _listBoxRowHeight = double.NaN;

    private SearchResult? SelectedSearchResult => (SearchResult?)_results.SelectedItem;

    public MainWindow(
        Settings settings,
        IServiceProvider serviceProvider,
        ILogger<MainWindow> logger,
        IDb db,
        CacheManagerManager cacheManagerManager,
        IUI ui
    )
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _db = db;
        _cacheManagerManager = cacheManagerManager;
        _ui = (UI)ui;

        cacheManagerManager.LoadingChanged += CacheManagerManager_LoadingChanged;

        InitializeComponent();

        if (!double.IsNaN(_listBoxRowHeight))
            RecalculateListBoxHeight();

        SetupShortcut();

        _notifyIcon = SetupNotifyIcon();
    }

    private void OpenSettings()
    {
        var window = _serviceProvider.GetRequiredService<ConfigurationUI.ConfigurationWindow>();
        window.Owner = this;
        window.ShowDialog();
    }

    private void CacheManagerManager_LoadingChanged(object sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                var isLoading = ((CacheManagerManager)sender).IsLoading;

                _spinner.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

                if (!isLoading)
                    _searchManager?.DoSearch();
            })
        );
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
#if DEBUG
        DoShow();
#endif
    }

    private void _keyboardHook_KeyPressed(object? sender, HotkeyPressedEventArgs e)
    {
        DoShow();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        _keyboardHook?.Dispose();
        _keyboardHook = null;

        _notifyIcon.Dispose();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.System when e.SystemKey == Key.F4:
                e.Handled = true;
                DoHide();
                break;
        }
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        // Detect whether a child dialog is being displayed.
        var haveChildWindow = Application.Current.Windows
            .OfType<Window>()
            .Any(p => p.Owner == this);

        if (!haveChildWindow)
            DoHide();
    }

    private void DoShow()
    {
        RepositionScreen();

        _search.Text = "";

        _results.ItemsSource = null;
        _results.Visibility = Visibility.Collapsed;

        _searchManager = _serviceProvider.GetRequiredService<SearchManager>();
        _searchManager.SearchResultsChanged += _searchManager_SearchResultsChanged;
        _searchManager.StackChanged += _searchManager_StackChanged;
        _searchManager.IsSearchingChanged += _searchManager_IsSearchingChanged;

        RenderStack();

        _ui.SetMainWindow(this);

        // Force recalculation of the height of the window.

        UpdateLayout();

        Visibility = Visibility.Visible;

        Activate();

        _search.Focus();
    }

    private void _searchManager_IsSearchingChanged(object sender, EventArgs e)
    {
        _dancingDots.Visibility =
            _searchManager?.IsSearching ?? false ? Visibility.Visible : Visibility.Collapsed;
    }

    private void _searchManager_SearchResultsChanged(object sender, EventArgs e)
    {
        if (_searchManager == null || _searchManager.Results.Length == 0)
        {
            _results.ItemsSource = null;
            _results.Visibility = Visibility.Collapsed;
            return;
        }

        _results.Visibility = Visibility.Visible;

        _results.ItemsSource = _searchManager.Results;

        if (_results.Items.Count > 0)
            _results.SelectedIndex = 0;
    }

    private void _searchManager_StackChanged(object sender, EventArgs e) => RenderStack();

    private void RenderStack()
    {
        if (_searchManager == null)
            return;

        if (_searchManager.Stack.Length == 0)
        {
            _stack.Visibility = Visibility.Collapsed;
            return;
        }

        _stack.Visibility = Visibility.Visible;

        _stackContainer.Children.Clear();

        foreach (var match in _searchManager.Stack)
        {
            if (_stackContainer.Children.Count > 0)
            {
                _stackContainer.Children.Add(
                    new TextBlock(new Run(" » ")) { FontSize = _search.FontSize }
                );
            }

            _stackContainer.Children.Add(
                SearchResultUtils.RenderMatch(match, null, false, _search.FontSize)
            );
        }
    }

    private void DoHide()
    {
        _ui.SetMainWindow(null);

        Visibility = Visibility.Hidden;

        if (_searchManager != null)
        {
            _searchManager.SearchResultsChanged -= _searchManager_SearchResultsChanged;
            _searchManager.StackChanged -= _searchManager_StackChanged;
            _searchManager.IsSearchingChanged -= _searchManager_IsSearchingChanged;
            _searchManager.Dispose();
            _searchManager = null;
        }
    }

    private void _search_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchManager?.SearchChanged(_search.Text);
    }

    private void _search_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var searchResult = SelectedSearchResult;

        switch (e.Key)
        {
            case Key.Up:
            case Key.Down:
                SelectItem(e.Key == Key.Up ? -1 : 1);
                e.Handled = true;
                break;

            case Key.PageUp:
            case Key.PageDown:
                SelectPage(e.Key == Key.PageUp ? -1 : 1);
                e.Handled = true;
                break;

            case Key.Enter:
            {
                if (searchResult?.Match is IRunnableMatch runnable)
                    RunItem(runnable, searchResult);
                else if (searchResult?.Match is ISearchableMatch searchable)
                    PushItem(searchable, searchResult);
                e.Handled = true;
                break;
            }

            case Key.Tab:
            {
                if (searchResult?.Match is ISearchableMatch searchable)
                    PushItem(searchable, searchResult);
                e.Handled = true;
                break;
            }

            case Key.Escape:
                if (_searchManager?.Stack.Length > 0)
                    PopItem();
                else
                    DoHide();
                e.Handled = true;
                break;

            case Key.Back:
                if (_search.Text.Length == 0 && _searchManager?.Stack.Length > 0)
                    PopItem();
                break;
        }
    }

    private void _results_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var searchResult = SelectedSearchResult;

        switch (e.Key)
        {
            case Key.Enter:
                if (searchResult?.Match is IRunnableMatch runnable)
                {
                    RunItem(runnable, searchResult);
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                _search.Focus();
                e.Handled = true;
                break;
        }
    }

    private void _results_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var searchResult = SelectedSearchResult;

        if (e.ChangedButton == MouseButton.Left && searchResult?.Match is IRunnableMatch runnable)
        {
            RunItem(runnable, searchResult);
            e.Handled = true;
        }
    }

    private void SelectItem(int offset)
    {
        if (_results.Items.Count == 0)
            return;

        var index = _results.SelectedIndex == -1 ? 0 : _results.SelectedIndex + offset;
        var newIndex = Math.Min(Math.Max(index, 0), _results.Items.Count - 1);

        _results.SelectedIndex = newIndex;

        _results.ScrollIntoView(_results.Items[newIndex]);
    }

    private void SelectPage(int i) => SelectItem(i * ResultItemsCount);

    private async void RunItem(IRunnableMatch match, SearchResult searchResult)
    {
        MarkAsAccessed(searchResult);

        try
        {
            await match.Run(_serviceProvider, this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run match");
        }

        DoHide();
    }

    private void PushItem(ISearchableMatch match, SearchResult searchResult)
    {
        MarkAsAccessed(searchResult);

        _searchManager?.SuspendSearch();

        _search.Text = string.Empty;

        _searchManager?.Push(match);

        _searchManager?.ResumeSearch();
    }

    private void MarkAsAccessed(SearchResult searchResult)
    {
        if (searchResult.Match is not ISerializableMatch match)
            return;

        var parentTypeId = _searchManager?.Stack.LastOrDefault()?.TypeId.Id;
        var json = match.Serialize();

        using var access = _db.Access();

        var historyId =
            searchResult.HistoryId
            ?? access.FindHistory(match.TypeId.PluginId, match.TypeId.Id, json);

        if (historyId.HasValue)
        {
            access.MarkHistoryAsAccessed(historyId.Value);
        }
        else
        {
            access.AddHistory(
                new HistoryEntity
                {
                    PluginId = match.TypeId.PluginId,
                    ParentTypeId = parentTypeId,
                    TypeId = match.TypeId.Id,
                    Json = json
                }
            );
        }
    }

    private void PopItem()
    {
        _searchManager?.SuspendSearch();

        _search.Text = string.Empty;

        _searchManager?.Pop();

        _searchManager?.ResumeSearch();
    }

    private void SearchResultUserControl_HistoryRemoved(object sender, EventArgs e)
    {
        var searchResult = SelectedSearchResult;
        if (searchResult?.HistoryId == null)
            return;

        using (var access = _db.Access())
        {
            access.DeleteHistory(searchResult.HistoryId.Value);
        }

        _searchManager?.DeleteHistory(searchResult.HistoryId.Value);

        _search.Focus();
    }

    private void SearchResultUserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        for (
            var current = (DependencyObject?)sender;
            current != null;
            current = VisualTreeHelper.GetParent(current)
        )
        {
            if (current is ListBoxItem listBoxItem)
            {
                // For some reason, we get two size changed events. The second
                // one is the right one, and is bigger than the first one, so
                // this logic filters for that.
                if (double.IsNaN(_listBoxRowHeight) || _listBoxRowHeight < listBoxItem.ActualHeight)
                {
                    _listBoxRowHeight = listBoxItem.ActualHeight;

                    RecalculateListBoxHeight();
                }
                return;
            }
        }
    }

    private void RecalculateListBoxHeight()
    {
        _results.Height =
            _listBoxRowHeight * ResultItemsCount
            + _results.BorderThickness.Top
            + _results.BorderThickness.Bottom;
    }
}
