using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.QuickStart;
using Tql.App.Search;
using Tql.App.Services;
using Tql.App.Services.Database;
using Tql.App.Services.Telemetry;
using Tql.App.Support;
using Tql.Utilities;

namespace Tql.App;

// Arguably, this class is doing too much. It would be better to
// focus this window on just the search capabilities, and remove
// everything around that (the keyboard hook, notification icon,
// etc) somewhere else. That would allow us to show the window
// only when the user requests a search.
//
// However, this causes significant slow down. I've tried this a
// few different ways, and it all causes noticeable delay in the
// search dialog becoming available. If you're fast enough (like
// I am), it's not fast enough to capture the first characters
// typed.

internal partial class MainWindow
{
    private const int ResultItemsCount = 8;

    private readonly Settings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindow> _logger;
    private readonly IDb _db;
    private readonly CacheManagerManager _cacheManagerManager;
    private readonly TelemetryService _telemetryService;
    private readonly QuickStartScript _quickStartScript;
    private readonly QuickStartManager _quickStartManager;
    private readonly NotifyIconManager _notifyIconManager;
    private readonly UI _ui;
    private double _listBoxRowHeight = double.NaN;
    private bool _pendingEnter;
    private SearchResult? _mouseDownSearchResult;
    private IPageViewTelemetry? _pageView;

    private SearchResult? SelectedSearchResult => (SearchResult?)_results.SelectedItem;

    public SearchManager? SearchManager { get; private set; }
    public Image ConfigurationImage => _configurationImage;

    public event EventHandler<MatchEventArgs>? MatchActivated;
    public event EventHandler<MatchEventArgs>? MatchPushed;
    public event EventHandler<MatchEventArgs>? MatchPopped;
    public event EventHandler<MatchEventArgs>? MatchCopied;
    public event EventHandler<MatchEventArgs>? MatchHistoryRemoved;
    public event EventHandler<MatchEventArgs>? MatchPinned;
    public event EventHandler<MatchEventArgs>? MatchUnpinned;

    public MainWindow(
        Settings settings,
        IServiceProvider serviceProvider,
        ILogger<MainWindow> logger,
        IDb db,
        CacheManagerManager cacheManagerManager,
        IUI ui,
        TelemetryService telemetryService,
        HotKeyService hotKeyService,
        QuickStartScript quickStartScript,
        QuickStartManager quickStartManager,
        NotifyIconManager notifyIconManager
    )
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _db = db;
        _cacheManagerManager = cacheManagerManager;
        _telemetryService = telemetryService;
        _quickStartScript = quickStartScript;
        _quickStartManager = quickStartManager;
        _notifyIconManager = notifyIconManager;
        _ui = (UI)ui;

        cacheManagerManager.LoadingChanged += CacheManagerManager_LoadingChanged;
        cacheManagerManager.CacheChanged += CacheManagerManager_CacheChanged;

        InitializeComponent();

        Tint = ParseMainWindowTint();

        settings.AttachPropertyChanged(
            nameof(settings.MainWindowTint),
            (_, _) => Tint = ParseMainWindowTint()
        );

        ApplyTextOuterGlow();

        _runImage.Source = Images.Run;
        _categoryImage.Source = Images.Category;

        _clearImage.Source = Images.Backspace;
        _clearImage.AttachOnClickHandler(() => DoShow(true));

        _feedbackImage.Source = Images.CommentNote;
        _feedbackImage.AttachOnClickHandler(OpenFeedback);
        _feedbackImage.SetPopoverToolTip(
            Labels.MainWindow_LeaveFeedback,
            Labels.MainWindow_LeaveFeedbackHelpText
        );

        _helpImage.Source = Images.Help;
        _helpImage.AttachOnClickHandler(OpenHelp);
        _helpImage.SetPopoverToolTip(Labels.MainWindow_HelpAndDocumentation);

        _configurationImage.Source = Images.Settings;
        _configurationImage.AttachOnClickHandler(() => OpenConfiguration());
        _configurationImage.SetPopoverToolTip(Labels.MainWindow_Configuration);

        _spinner.SetPopoverToolTip(
            Labels.MainWindow_CacheIsBeingUpdated,
            Labels.MainWindow_CacheIsBeingUpdatedHelpText
        );

        if (!double.IsNaN(_listBoxRowHeight))
            RecalculateListBoxHeight();

        _notifyIconManager.ContextMenuStrip = SetupNotifyIconContextMenu();
        _notifyIconManager.Clicked += (_, _) => DoShow();

        ResetFontSize();

        settings.AttachPropertyChanged(nameof(settings.MainFontSize), (_, _) => ResetFontSize());
        settings.AttachPropertyChanged(nameof(settings.HotKey), (_, _) => RenderHotKey());

        _ui.UINotificationsChanged += (_, _) => ReloadNotifications();
        _ui.ConfigurationUIRequested += (_, e) => OpenConfiguration(e.Id);

        hotKeyService.Pressed += (_, _) => DoShow();

        RenderHotKey();
    }

    private Color ParseMainWindowTint()
    {
        var mainWindowTint = _settings.MainWindowTint;

        return SettingsUtils.ParseMainWindowTint(mainWindowTint);
    }

    private void ApplyTextOuterGlow()
    {
        ApplySetting();

        _settings.AttachPropertyChanged(
            nameof(_settings.TextOuterGlowSize),
            (_, _) => ApplySetting()
        );

        void ApplySetting()
        {
            var textOuterGlowSize =
                _settings.TextOuterGlowSize ?? Settings.DefaultTextOuterGlowSize;

            Resources.Remove("TextOuterGlowBlurRadius");
            Resources.Add("TextOuterGlowBlurRadius", (double)textOuterGlowSize);
        }
    }

    private void RenderHotKey()
    {
        var hotKey = HotKey.FromSettings(_settings);

        _hotKeyWin.Visibility = hotKey.Win ? Visibility.Visible : Visibility.Collapsed;
        _hotKeyControl.Visibility = hotKey.Control ? Visibility.Visible : Visibility.Collapsed;
        _hotKeyAlt.Visibility = hotKey.Alt ? Visibility.Visible : Visibility.Collapsed;
        _hotKeyShift.Visibility = hotKey.Shift ? Visibility.Visible : Visibility.Collapsed;

        var keyLabel = HotKey.AvailableKeys.Single(p => p.Key == hotKey.Key).Label;

        _hotKeyName.Inlines.Clear();
        _hotKeyName.Inlines.Add(keyLabel);
    }

    private void ResetFontSize()
    {
        var fontSize = _settings.MainFontSize ?? Settings.DefaultMainFontSize;

        _listBoxRowHeight = double.NaN;

        _search.FontSize = WpfUtils.PointsToPixels(fontSize);
        _results.FontSize = WpfUtils.PointsToPixels(fontSize);
    }

    private void OpenConfiguration(Guid? id = null)
    {
        if (!IsVisible)
            DoShow();

        var window = _serviceProvider.GetRequiredService<ConfigurationUI.ConfigurationWindow>();
        window.Owner = this;
        window.StartupPage = id;

        _ui.EnterModalDialog();
        try
        {
            window.ShowDialog();
        }
        finally
        {
            _ui.ExitModalDialog();
        }

        if (!App.IsShuttingDown)
            _quickStartScript.HandleMainWindow(this);

        // The settings may have impacted the current search results.
        DoShow(true);
    }

    private void OpenFeedback()
    {
        var window = _serviceProvider.GetRequiredService<FeedbackWindow>();
        window.Owner = this;

        _ui.EnterModalDialog();
        try
        {
            window.ShowDialog();
        }
        finally
        {
            _ui.ExitModalDialog();
        }
    }

    private void OpenHelp()
    {
        _ui.OpenUrl(Constants.HelpUrl);
    }

    private void CacheManagerManager_LoadingChanged(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(UpdateCacheManagerSpinner));
    }

    private void UpdateCacheManagerSpinner()
    {
        _spinner.Visibility = _cacheManagerManager.IsLoading
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void CacheManagerManager_CacheChanged(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(Invoke));

        void Invoke()
        {
            if (IsVisible)
                SearchManager?.DoSearch();
        }
    }

    private void Window_PreviewKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.System when e.SystemKey == Key.F4:
                e.Handled = true;
                DoHide();
                break;
        }
    }

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        DoHide();
    }

    public void DoShow() => DoShow(false);

    private void DoShow(bool force)
    {
        if (IsVisible && !force)
        {
            SearchManager?.DoSearch();

            Activate();

            _search.Focus();
            return;
        }

        _pageView ??= _telemetryService.CreatePageView("Search");

        RepositionScreen();

        _dancingDots.Visibility = Visibility.Collapsed;

        _search.Text = "";

        _pendingEnter = false;

        _results.ItemsSource = null;

        SetResultsVisibility(Visibility.Collapsed);

        DisposeSearchManager();

        SearchManager = _serviceProvider.GetRequiredService<SearchManager>();
        SearchManager.SearchCompleted += _searchManager_SearchCompleted;
        SearchManager.StackChanged += _searchManager_StackChanged;
        SearchManager.IsSearchingChanged += _searchManager_IsSearchingChanged;

        RenderStack();

        _ui.SetMainWindow(this);

        UpdateClearVisibility();

        ReloadNotifications();

        UpdateCacheManagerSpinner();

        // Force recalculation of the height of the window.

        UpdateLayout();

        Show();

        Activate();

        Dispatcher.BeginInvoke(
            () => _quickStartScript.HandleMainWindow(this),
            DispatcherPriority.Loaded
        );

        _search.Focus();
    }

    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        //
        // HACK: Sometimes when the app starts up the first time, the content
        // of the window isn't rendered. Resizing the window fixes this. This
        // bug can be forced by replacing the content of this method with
        // setting the Height to 0.
        //
        // This suggestion came from ChatGPT and consistently fixes the issue.
        // This does show a slight delay in the content being rendered, on first
        // show only. There is no impact on subsequent show calls.
        //

        Opacity = 0;
        Dispatcher.BeginInvoke(() => Opacity = 1, DispatcherPriority.Render);
    }

    private void ReloadNotifications()
    {
        _notificationBars.ItemsSource = _ui.UINotifications;
    }

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        RepositionScreen();

        Activate();
    }

    private void _searchManager_IsSearchingChanged(object? sender, EventArgs e)
    {
        var isSearching = SearchManager?.IsSearching ?? false;

        _dancingDots.Visibility = isSearching ? Visibility.Visible : Visibility.Collapsed;

        if (isSearching)
            _pendingEnter = false;
    }

    private void _searchManager_SearchCompleted(object? sender, SearchResultsEventArgs e)
    {
        if (e.Results.Length == 0)
        {
            _results.ItemsSource = null;
            if (!e.IsPreliminary)
                SetResultsVisibility(Visibility.Collapsed);
            return;
        }

        SetResultsVisibility(Visibility.Visible);

        _results.ItemsSource = e.Results;

        if (_results.Items.Count > 0)
        {
            SetSelectedIndex(0);

            if (_pendingEnter && _results.Items.Count == 1)
            {
                var searchResult = SelectedSearchResult;
                if (searchResult?.Match is IRunnableMatch runnable)
                    RunItem(runnable, searchResult);
            }
        }

        _pendingEnter = false;
    }

    private void SetResultsVisibility(Visibility visibility)
    {
        _results.Visibility = visibility;
        _resultsSeparator.Visibility = visibility;
    }

    private void _searchManager_StackChanged(object? sender, EventArgs e)
    {
        RenderStack();

        UpdateClearVisibility();
    }

    private void RenderStack()
    {
        if (SearchManager == null)
            return;

        var searchHint = default(string);

        if (SearchManager.Stack.Length == 0)
        {
            _stack.Visibility = Visibility.Collapsed;
        }
        else
        {
            _stack.Visibility = Visibility.Visible;

            _stack.Child = SearchResultUtils.RenderMatch(
                SearchManager.Stack.Last(),
                null,
                false,
                _search.FontSize,
                maxWidth: _stack.MaxWidth
                    - (
                        _stack.Padding.Left
                        + _stack.Padding.Right
                        + _stack.BorderThickness.Left
                        + _stack.BorderThickness.Right
                    )
            );

            searchHint = SearchManager.Stack.Last().SearchHint;
        }

        // The HintedTextBox style expects the hint in the Tag.
        _search.Tag = searchHint;
    }

    private void DoHide()
    {
        if (_ui.IsModalDialogShowing)
            return;

        // Detect whether a child dialog is being displayed.

        var haveChildWindow = false;
        var haveQuickStartChildWindow = false;

        foreach (
            var window in Application.Current.Windows.OfType<Window>().Where(p => p.Owner == this)
        )
        {
            if (window is QuickStartWindow)
                haveQuickStartChildWindow = true;
            else
                haveChildWindow = true;
        }

#if DEBUG
        if (haveQuickStartChildWindow)
            haveChildWindow = true;
#endif

        if (haveChildWindow || _ui.IsModalDialogShowing)
            return;

        if (haveQuickStartChildWindow)
            _quickStartManager.Close();

        _ui.SetMainWindow(null);

        Visibility = Visibility.Hidden;

        DisposeSearchManager();

        _pageView?.Dispose();
        _pageView = null;
    }

    private void DisposeSearchManager()
    {
        if (SearchManager != null)
        {
            SearchManager.SearchCompleted -= _searchManager_SearchCompleted;
            SearchManager.StackChanged -= _searchManager_StackChanged;
            SearchManager.IsSearchingChanged -= _searchManager_IsSearchingChanged;
            SearchManager.Dispose();
            SearchManager = null;
        }
    }

    private void _search_TextChanged(object? sender, TextChangedEventArgs e)
    {
        _pendingEnter = false;
        SearchManager?.SetSearch(_search.Text);

        UpdateClearVisibility();
    }

    private void UpdateClearVisibility()
    {
        _clearImage.Visibility =
            _search.Text.IsEmpty() && SearchManager?.Stack.Length == 0
                ? Visibility.Collapsed
                : Visibility.Visible;
    }

    private void _search_PreviewKeyDown(object? sender, KeyEventArgs e) =>
        HandlePreviewKeyDown(e, true);

    private void _results_PreviewKeyDown(object? sender, KeyEventArgs e) =>
        HandlePreviewKeyDown(e, false);

    private void HandlePreviewKeyDown(KeyEventArgs e, bool inSearch)
    {
        var searchResult = SelectedSearchResult;

        switch (e.Key)
        {
            case Key.Up
            or Key.Down when inSearch:
                SelectItem(e.Key == Key.Up ? -1 : 1);
                e.Handled = true;
                break;

            case Key.PageUp
            or Key.PageDown when inSearch:
                SelectPage(e.Key == Key.PageUp ? -1 : 1);
                e.Handled = true;
                break;

            case Key.Enter:
                if (
                    searchResult == null
                    && SearchManager
                        is { IsSearching: true, Context.IsPreliminaryResultsSuppressed: true }
                )
                    _pendingEnter = true;
                else if (searchResult?.Match is IRunnableMatch runnable)
                    RunItem(runnable, searchResult);
                else if (searchResult?.Match is ISearchableMatch searchable1)
                    PushItem(searchable1, searchResult);
                if (!inSearch)
                    _search.Focus();
                e.Handled = true;
                break;

            case Key.Tab:
                if (searchResult?.Match is ISearchableMatch searchable2)
                    PushItem(searchable2, searchResult);
                if (!inSearch)
                    _search.Focus();
                e.Handled = true;
                break;

            case Key.Escape:
                if (_search.Text.Length > 0)
                    _search.Text = null;
                else if (SearchManager?.Stack.Length > 0)
                    PopItem();
                else if (inSearch)
                    DoHide();
                else
                    _search.Focus();
                e.Handled = true;
                break;

            case Key.Back:
                if (_search.Text.Length == 0 && SearchManager?.Stack.Length > 0)
                {
                    PopItem();
                    if (!inSearch)
                        _search.Focus();
                    e.Handled = true;
                }
                break;
        }
    }

    private void _results_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        _mouseDownSearchResult = null;

        // Getting the search result the user is selecting is fiddly
        // because we're in the preview event. The list box item
        // won't yet be the selected one.

        if (e.OriginalSource is DependencyObject source)
        {
            var control = source.FindVisualParent<SearchResultUserControl>();
            if (control != null)
                _mouseDownSearchResult = control.DataContext as SearchResult;
        }
    }

    private void _results_MouseUp(object? sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        // Match the selected search result with what we captured on mouse down.
        // We can receive mouse up's without a mouse down, if the mouse down
        // started outside of the window. So, reset the _mouseDownSearchResult
        // and hope this logic is correct.

        var searchResult = SelectedSearchResult;
        var isMouseDownSearchResult = searchResult == _mouseDownSearchResult;
        _mouseDownSearchResult = null;

        if (!isMouseDownSearchResult)
            return;

        if (searchResult?.Match is IRunnableMatch runnable)
        {
            RunItem(runnable, searchResult);
            e.Handled = true;
        }
        if (searchResult?.Match is ISearchableMatch searchable)
        {
            PushItem(searchable, searchResult);
            _search.Focus();
            e.Handled = true;
        }
    }

    private void SelectItem(int offset)
    {
        if (_results.Items.Count == 0)
            return;

        var index = _results.SelectedIndex == -1 ? 0 : _results.SelectedIndex + offset;
        var newIndex = Math.Min(Math.Max(index, 0), _results.Items.Count - 1);

        SetSelectedIndex(newIndex);
    }

    private void SetSelectedIndex(int newIndex)
    {
        _results.SelectedIndex = newIndex;
        _results.ScrollIntoView(_results.Items[newIndex]!);
    }

    private void SelectPage(int i) => SelectItem(i * ResultItemsCount);

    private async void RunItem(IRunnableMatch match, SearchResult searchResult)
    {
        using var telemetry = _telemetryService.CreateDependency("Run");

        match.InitializeTelemetry(telemetry);

        MarkAsAccessed(searchResult);

        OnMatchActivated(new MatchEventArgs(match));

        try
        {
            var handle = new WindowInteropHelper(this).Handle;

            await match.Run(_serviceProvider, new Win32Window(handle));

            telemetry.IsSuccess = true;
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

        OnMatchPushed(new MatchEventArgs(searchResult.Match));

        SearchManager?.SuspendSearch();

        _search.Text = string.Empty;

        SearchManager?.Push(match);

        SearchManager?.ResumeSearch();
    }

    private async void CopyItem(ICopyableMatch match, SearchResult searchResult)
    {
        using (var telemetry = _telemetryService.CreateEvent("Copy Item"))
        {
            match.InitializeTelemetry(telemetry);
        }

        OnMatchCopied(new MatchEventArgs(match));

        MarkAsAccessed(searchResult);

        try
        {
            await match.Copy(_serviceProvider);

            _snackbar.Text = Labels.MainWindow_LinkCopied;
            _snackbar.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy match");
        }
    }

    private long? MarkAsAccessed(SearchResult searchResult)
    {
        if (searchResult.Match is not ISerializableMatch match)
            return null;

        var parent = SearchManager?.Stack.LastOrDefault();
        var parentTypeId = parent?.TypeId.Id;
        var parentJson = (parent as ISerializableMatch)?.Serialize();
        var json = match.Serialize();

        using var access = _db.Access();

        var historyId =
            searchResult.HistoryId
            ?? access.FindHistory(match.TypeId.PluginId, match.TypeId.Id, json);

        if (historyId.HasValue)
        {
            // TODO: This is a temporary solution. It's to migrate the ParentJson
            // of the history if the one returned by the plugin changes.
            // There's an outstanding item to do this better.
            access.MarkHistoryAsAccessed(historyId.Value, parentJson);
        }
        else
        {
            using (var telemetry = _telemetryService.CreateEvent("History Created"))
            {
                match.InitializeTelemetry(telemetry);
            }

            access.AddHistory(
                new HistoryEntity
                {
                    PluginId = match.TypeId.PluginId,
                    ParentTypeId = parentTypeId,
                    ParentJson = parentJson,
                    TypeId = match.TypeId.Id,
                    Json = json
                }
            );
        }

        return historyId;
    }

    private void PopItem()
    {
        if (SearchManager?.Stack.Length > 0)
            OnMatchPopped(new MatchEventArgs(SearchManager.Stack.Last()));

        SearchManager?.SuspendSearch();

        _search.Text = string.Empty;

        SearchManager?.Pop();

        SearchManager?.ResumeSearch();
    }

    private void SearchResultUserControl_RemoveHistoryClicked(object? sender, EventArgs e)
    {
        var searchResult = SelectedSearchResult;
        if (searchResult?.HistoryId == null)
            return;

        using (var telemetry = _telemetryService.CreateEvent("History Deleted"))
        {
            searchResult.Match.InitializeTelemetry(telemetry);
        }

        OnMatchHistoryRemoved(new MatchEventArgs(searchResult.Match));

        using (var access = _db.Access())
        {
            access.DeleteHistory(searchResult.HistoryId.Value);
        }

        ReloadResults();
    }

    private void SearchResultUserControl_SizeChanged(object? sender, SizeChangedEventArgs e)
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

    private void SearchResultUserControl_CopyClicked(object? sender, EventArgs e)
    {
        var searchResult = SelectedSearchResult;
        if (searchResult?.Match is ICopyableMatch match)
            CopyItem(match, searchResult);

        _search.Focus();
    }

    private void SearchResultUserControl_PinClicked(object? sender, EventArgs e) => SetPinned(true);

    private void SearchResultUserControl_UnpinClicked(object? sender, EventArgs e) =>
        SetPinned(false);

    private void SetPinned(bool pinned)
    {
        var searchResult = SelectedSearchResult;
        if (searchResult == null)
            return;

        if (!searchResult.HistoryId.HasValue && !pinned)
            return;

        var historyId = searchResult.HistoryId ?? MarkAsAccessed(searchResult);
        if (historyId == null)
            return;

        using (var telemetry = _telemetryService.CreateEvent(pinned ? "Pinned" : "Unpinned"))
        {
            searchResult.Match.InitializeTelemetry(telemetry);
        }

        if (pinned)
            OnMatchPinned(new MatchEventArgs(searchResult.Match));
        else
            OnMatchUnpinned(new MatchEventArgs(searchResult.Match));

        using (var access = _db.Access())
        {
            access.SetHistoryPinned(historyId.Value, pinned);
        }

        ReloadResults();
    }

    private void ReloadResults()
    {
        SearchManager?.ReloadHistory();

        _search.Focus();
    }

    private void NotificationBarUserControl_Activated(object? sender, UINotificationEventArgs e)
    {
        _ui.RemoveNotificationBar(e.Notification.Key);

        ReloadNotifications();

        e.Notification.Activate?.Invoke();
    }

    private void NotificationBarUserControl_Dismissed(object? sender, UINotificationEventArgs e)
    {
        _ui.RemoveNotificationBar(e.Notification.Key);

        ReloadNotifications();

        e.Notification.Dismiss?.Invoke();
    }

    protected virtual void OnMatchActivated(MatchEventArgs e) => MatchActivated?.Invoke(this, e);

    protected virtual void OnMatchPushed(MatchEventArgs e) => MatchPushed?.Invoke(this, e);

    protected virtual void OnMatchPopped(MatchEventArgs e) => MatchPopped?.Invoke(this, e);

    protected virtual void OnMatchCopied(MatchEventArgs e) => MatchCopied?.Invoke(this, e);

    protected virtual void OnMatchHistoryRemoved(MatchEventArgs e) =>
        MatchHistoryRemoved?.Invoke(this, e);

    protected virtual void OnMatchPinned(MatchEventArgs e) => MatchPinned?.Invoke(this, e);

    protected virtual void OnMatchUnpinned(MatchEventArgs e) => MatchUnpinned?.Invoke(this, e);
}
