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
    private readonly QuickStartManager _quickStart;
    private SearchManager? _searchManager;
    private readonly UI _ui;
    private double _listBoxRowHeight = double.NaN;
    private bool _pendingEnter;
    private SearchResult? _mouseDownSearchResult;
    private IPageViewTelemetry? _pageView;

    private SearchResult? SelectedSearchResult => (SearchResult?)_results.SelectedItem;

    public MainWindow(
        Settings settings,
        IServiceProvider serviceProvider,
        ILogger<MainWindow> logger,
        IDb db,
        CacheManagerManager cacheManagerManager,
        IUI ui,
        TelemetryService telemetryService,
        HotKeyService hotKeyService,
        QuickStartManager quickStart
    )
        : base(settings)
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _db = db;
        _cacheManagerManager = cacheManagerManager;
        _telemetryService = telemetryService;
        _quickStart = quickStart;
        _ui = (UI)ui;

        cacheManagerManager.LoadingChanged += CacheManagerManager_LoadingChanged;
        cacheManagerManager.CacheChanged += CacheManagerManager_CacheChanged;

        InitializeComponent();

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

        _settingsImage.Source = Images.Settings;
        _settingsImage.AttachOnClickHandler(() => OpenSettings());
        _settingsImage.SetPopoverToolTip(Labels.MainWindow_Settings);

        _spinner.SetPopoverToolTip(
            Labels.MainWindow_CacheIsBeingUpdated,
            Labels.MainWindow_CacheIsBeingUpdatedHelpText
        );

        if (!double.IsNaN(_listBoxRowHeight))
            RecalculateListBoxHeight();

        _notifyIcon = SetupNotifyIcon();

        ResetFontSize();

        settings.AttachPropertyChanged(nameof(settings.MainFontSize), (_, _) => ResetFontSize());
        settings.AttachPropertyChanged(nameof(settings.HotKey), (_, _) => RenderHotKey());

        _ui.UINotificationsChanged += (_, _) => ReloadNotifications();
        _ui.ConfigurationUIRequested += (_, e) => OpenSettings(e.Id);

        hotKeyService.Pressed += (_, _) => DoShow();

        RenderHotKey();
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

    private void OpenSettings(Guid? id = null)
    {
        if (!IsVisible)
            DoShow();

        var window = _serviceProvider.GetRequiredService<ConfigurationUI.ConfigurationWindow>();
        window.Owner = this;
        window.StartupPage = id;
        window.ShowDialog();

        // The settings may have impacted the current search results.
        _searchManager?.DoSearch();
    }

    private void OpenFeedback()
    {
        var window = _serviceProvider.GetRequiredService<FeedbackWindow>();
        window.Owner = this;
        window.ShowDialog();
    }

    private void OpenHelp()
    {
        _ui.OpenUrl(Constants.HelpUrl);
    }

    private void CacheManagerManager_LoadingChanged(object sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(UpdateCacheManagerSpinner));
    }

    private void UpdateCacheManagerSpinner()
    {
        _spinner.Visibility = _cacheManagerManager.IsLoading
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void CacheManagerManager_CacheChanged(object sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(Invoke));

        void Invoke()
        {
            if (IsVisible)
                _searchManager?.DoSearch();
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
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

        if (!haveChildWindow && !_ui.IsShowingTaskDialog)
            DoHide();
    }

    public void DoShow() => DoShow(false);

    private void DoShow(bool force)
    {
        if (IsVisible && !force)
        {
            _searchManager?.DoSearch();

            Activate();

            _search.Focus();
            return;
        }

        _pageView ??= _telemetryService.CreatePageView("Search");

        RepositionScreen();

        _search.Text = "";

        _pendingEnter = false;

        _results.ItemsSource = null;

        SetResultsVisibility(Visibility.Collapsed);

        DisposeSearchManager();

        _searchManager = _serviceProvider.GetRequiredService<SearchManager>();
        _searchManager.SearchResultsChanged += _searchManager_SearchResultsChanged;
        _searchManager.StackChanged += _searchManager_StackChanged;
        _searchManager.IsSearchingChanged += _searchManager_IsSearchingChanged;

        RenderStack();

        _ui.SetMainWindow(this);

        UpdateClearVisibility();

        ReloadNotifications();

        UpdateCacheManagerSpinner();

        // Force recalculation of the height of the window.

        UpdateLayout();

        Show();

        Activate();

        HandleQuickStart();

        _search.Focus();
    }

    private void ReloadNotifications()
    {
        _notificationBars.ItemsSource = _ui.UINotifications;
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        RepositionScreen();

        Activate();
    }

    private void _searchManager_IsSearchingChanged(object sender, EventArgs e)
    {
        var isSearching = _searchManager?.IsSearching ?? false;

        _dancingDots.Visibility = isSearching ? Visibility.Visible : Visibility.Collapsed;

        if (isSearching)
            _pendingEnter = false;
    }

    private void _searchManager_SearchResultsChanged(object sender, EventArgs e)
    {
        if (_searchManager == null || _searchManager.Results.Length == 0)
        {
            _results.ItemsSource = null;
            SetResultsVisibility(Visibility.Collapsed);
            return;
        }

        SetResultsVisibility(Visibility.Visible);

        _results.ItemsSource = _searchManager.Results;

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

    private void _searchManager_StackChanged(object sender, EventArgs e)
    {
        RenderStack();

        UpdateClearVisibility();
    }

    private void RenderStack()
    {
        if (_searchManager == null)
            return;

        var searchHint = default(string);

        if (_searchManager.Stack.Length == 0)
        {
            _stack.Visibility = Visibility.Collapsed;
        }
        else
        {
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
                    SearchResultUtils.RenderMatch(
                        match,
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
                    )
                );
            }

            searchHint = (_searchManager.Stack.Last() as IHasSearchHint)?.SearchHint;
        }

        // The HintedTextBox style expects the hint in the Tag.
        _search.Tag = searchHint;
    }

    private void DoHide()
    {
        _ui.SetMainWindow(null);

        Visibility = Visibility.Hidden;

        DisposeSearchManager();

        _pageView?.Dispose();
        _pageView = null;
    }

    private void DisposeSearchManager()
    {
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
        _pendingEnter = false;
        _searchManager?.SearchChanged(_search.Text);

        UpdateClearVisibility();
    }

    private void UpdateClearVisibility()
    {
        _clearImage.Visibility =
            _search.Text.IsEmpty() && _searchManager?.Stack.Length == 0
                ? Visibility.Collapsed
                : Visibility.Visible;
    }

    private void _search_PreviewKeyDown(object sender, KeyEventArgs e) =>
        HandlePreviewKeyDown(e, true);

    private void _results_PreviewKeyDown(object sender, KeyEventArgs e) =>
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
                    && _searchManager
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
                else if (_searchManager?.Stack.Length > 0)
                    PopItem();
                else if (inSearch)
                    DoHide();
                else
                    _search.Focus();
                e.Handled = true;
                break;

            case Key.Back:
                if (_search.Text.Length == 0 && _searchManager?.Stack.Length > 0)
                {
                    PopItem();
                    if (!inSearch)
                        _search.Focus();
                    e.Handled = true;
                }
                break;
        }
    }

    private void _results_PreviewMouseDown(object sender, MouseButtonEventArgs e)
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

    private void _results_MouseUp(object sender, MouseButtonEventArgs e)
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
        _results.ScrollIntoView(_results.Items[newIndex]);
    }

    private void SelectPage(int i) => SelectItem(i * ResultItemsCount);

    private async void RunItem(IRunnableMatch match, SearchResult searchResult)
    {
        using var telemetry = _telemetryService.CreateDependency("Run");

        match.InitializeTelemetry(telemetry);

        MarkAsAccessed(searchResult);

        try
        {
            await match.Run(_serviceProvider, this);

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

        _searchManager?.SuspendSearch();

        _search.Text = string.Empty;

        _searchManager?.Push(match);

        _searchManager?.ResumeSearch();
    }

    private async void CopyItem(ICopyableMatch match, SearchResult searchResult)
    {
        using (var telemetry = _telemetryService.CreateEvent("Copy Item"))
        {
            match.InitializeTelemetry(telemetry);
        }

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

        var parent = _searchManager?.Stack.LastOrDefault();
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
        _searchManager?.SuspendSearch();

        _search.Text = string.Empty;

        _searchManager?.Pop();

        _searchManager?.ResumeSearch();
    }

    private void SearchResultUserControl_RemoveHistoryClicked(object sender, EventArgs e)
    {
        var searchResult = SelectedSearchResult;
        if (searchResult?.HistoryId == null)
            return;

        using (var telemetry = _telemetryService.CreateEvent("History Deleted"))
        {
            searchResult.Match.InitializeTelemetry(telemetry);
        }

        using (var access = _db.Access())
        {
            access.DeleteHistory(searchResult.HistoryId.Value);
        }

        ReloadResults();
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

    private void SearchResultUserControl_CopyClicked(object sender, EventArgs e)
    {
        var searchResult = SelectedSearchResult;
        if (searchResult?.Match is ICopyableMatch match)
            CopyItem(match, searchResult);

        _search.Focus();
    }

    private void SearchResultUserControl_PinClicked(object sender, EventArgs e) => SetPinned(true);

    private void SearchResultUserControl_UnpinClicked(object sender, EventArgs e) =>
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

        using (var access = _db.Access())
        {
            access.SetHistoryPinned(historyId.Value, pinned);
        }

        ReloadResults();
    }

    private void ReloadResults()
    {
        _searchManager?.ReloadHistory();

        _search.Focus();
    }

    private void NotificationBarUserControl_Activated(object sender, UINotificationEventArgs e)
    {
        _ui.RemoveNotificationBar(e.Notification.Key);

        ReloadNotifications();

        e.Notification.Activate?.Invoke();
    }

    private void NotificationBarUserControl_Dismissed(object sender, UINotificationEventArgs e)
    {
        _ui.RemoveNotificationBar(e.Notification.Key);

        ReloadNotifications();

        e.Notification.Dismiss?.Invoke();
    }

    private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // This is here for debugging only. There's a strange scenario where
        // the main window pops up looking wrong. Likely it's uninitialized.

        if ((bool)e.NewValue && _searchManager == null)
        {
            _logger.LogError("Main window got visible unexpectedly");
            _logger.LogError(Environment.StackTrace);
        }
    }
}
