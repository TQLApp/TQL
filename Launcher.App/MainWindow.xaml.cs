using Launcher.Abstractions;
using Launcher.App.Interop;
using Launcher.App.Search;
using Launcher.App.Services;
using Launcher.App.Services.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Image = System.Windows.Controls.Image;

namespace Launcher.App;

internal partial class MainWindow
{
    private static DrawingImage LoadImage(string resourceName, Brush? fill = null)
    {
        using var stream = typeof(MainWindow).Assembly.GetManifestResourceStream(
            $"{typeof(MainWindow).Namespace}.Resources.{resourceName}"
        );

        return ImageFactory.CreateSvgImage(stream!, fill);
    }

    private readonly Settings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindow> _logger;
    private readonly IDb _db;
    private KeyboardHook? _keyboardHook;
    private SearchManager? _searchManager;
    private readonly TextDecoration _textDecoration;
    private readonly DrawingImage _runImage = LoadImage("Person Running.svg");
    private readonly DrawingImage _starImage = LoadImage("Star.svg");
    private readonly DrawingImage _dismissImage = LoadImage("Dismiss.svg");
    private readonly DrawingImage _categoryImage = LoadImage("Apps List.svg");

    private SearchResult? SelectedSearchResult
    {
        get
        {
            var listBoxItem = (ListBoxItem)_results.SelectedItem;
            return (SearchResult?)listBoxItem?.Tag;
        }
    }

    public MainWindow(
        Settings settings,
        IServiceProvider serviceProvider,
        ILogger<MainWindow> logger,
        IDb db,
        CacheManagerManager cacheManagerManager
    )
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _db = db;

        cacheManagerManager.LoadingChanged += CacheManagerManager_LoadingChanged;

        InitializeComponent();

        _textDecoration = new TextDecoration
        {
            Location = TextDecorationLocation.Underline,
            Pen = new Pen((Brush)_results.FindResource("WavyBrush"), 6)
        };

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

    private void Window_Deactivated(object sender, EventArgs e) => DoHide();

    private void DoShow()
    {
        RepositionScreen();

        _search.Text = "";

        _results.Items.Clear();
        _results.Visibility = Visibility.Collapsed;

        _searchManager = _serviceProvider.GetRequiredService<SearchManager>();
        _searchManager.SearchResultsChanged += _searchManager_SearchResultsChanged;
        _searchManager.StackChanged += _searchManager_StackChanged;

        RenderStack();

        // Force recalculation of the height of the window.

        UpdateLayout();

        Visibility = Visibility.Visible;

        Activate();

        _search.Focus();
    }

    private void _searchManager_SearchResultsChanged(object sender, EventArgs e)
    {
        if (_searchManager == null || _searchManager.Results.Length == 0)
        {
            _results.Items.Clear();
            _results.Visibility = Visibility.Collapsed;
            return;
        }

        _results.Visibility = Visibility.Visible;

        _results.Items.Clear();

        foreach (var result in _searchManager.Results)
        {
            var listBoxItem = new MyListBoxItem
            {
                VerticalAlignment = VerticalAlignment.Center,
                IsSelected = _results.Items.Count == 0,
                Tag = result
            };

            listBoxItem.IsMouseOverOrSelectedChanged += ListBoxItem_IsMouseOverOrSelectedChanged;

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2)
            };

            var iconsStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 6, 0)
            };

            Grid.SetColumn(iconsStackPanel, 1);

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children = { stackPanel, iconsStackPanel },
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            listBoxItem.Content = grid;
            listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            RenderMatch(stackPanel.Children, result.Match, result.TextMatch, result.IsFuzzyMatch);

            RenderMatchIcons(listBoxItem);

            _results.Items.Add(listBoxItem);
        }
    }

    private void ListBoxItem_IsMouseOverOrSelectedChanged(object sender, EventArgs e)
    {
        RenderMatchIcons((ListBoxItem)sender);
    }

    private void RenderMatchIcons(ListBoxItem listBoxItem)
    {
        var grid = (Grid)listBoxItem.Content;
        var iconsStackPanel = (StackPanel)grid.Children[grid.Children.Count - 1];
        var searchResult = (SearchResult)listBoxItem.Tag;

        iconsStackPanel.Children.Clear();

        if (listBoxItem.IsMouseOver || listBoxItem.IsSelected)
        {
            if (searchResult.Match is IRunnableMatch)
                AddIcon(_runImage);
            if (searchResult.Match is ISearchableMatch)
                AddIcon(_categoryImage);
        }

        if (searchResult.HistoryId.HasValue)
        {
            var star = AddIcon(_starImage);
            var dismiss = AddIcon(_dismissImage);

            star.MouseEnter += (_, _) =>
            {
                star.Visibility = Visibility.Collapsed;
                dismiss.Visibility = Visibility.Visible;
            };

            dismiss.MouseLeave += (_, _) =>
            {
                dismiss.Visibility = Visibility.Collapsed;
                star.Visibility = Visibility.Visible;
            };

            dismiss.Cursor = Cursors.Hand;
            dismiss.Visibility = Visibility.Collapsed;

            dismiss.MouseDown += (_, _) => dismiss.CaptureMouse();

            dismiss.MouseUp += (_, e) =>
            {
                if (dismiss.IsMouseOver)
                {
                    using (var access = _db.Access())
                    {
                        access.DeleteHistory(searchResult.HistoryId.Value);
                    }

                    _searchManager?.DeleteHistory(searchResult.HistoryId.Value);
                }

                dismiss.ReleaseMouseCapture();

                e.Handled = true;

                _search.Focus();
            };
        }

        Image AddIcon(DrawingImage icon)
        {
            var image = new Image
            {
                Source = icon,
                Width = 14,
                Height = 14,
                Margin = new Thickness(6, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            iconsStackPanel.Children.Add(image);

            return image;
        }
    }

    private void RenderMatch(
        UIElementCollection collection,
        IMatch match,
        TextMatch? textMatch,
        bool isFuzzyMatch
    )
    {
        collection.Add(
            new Image
            {
                Source = ((Services.Image)match.Icon).ImageSource,
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            }
        );

        var textBlock = new TextBlock { FontSize = _search.FontSize };

        collection.Add(textBlock);

        var offset = 0;

        var text = match.Text;

        if (textMatch != null)
        {
            foreach (var range in textMatch.Ranges)
            {
                if (offset < range.Offset)
                {
                    var part = text.Substring(offset, range.Offset - offset);
                    textBlock.Inlines.Add(new Run(part));
                }

                if (range.Length > 0)
                {
                    var part = text.Substring(range.Offset, range.Length);
                    var inline = new Bold(new Run(part));

                    if (isFuzzyMatch)
                        inline.TextDecorations.Add(_textDecoration);

                    textBlock.Inlines.Add(inline);
                }

                offset = range.Offset + range.Length;
            }
        }

        if (offset < text.Length)
        {
            var part = text.Substring(offset);
            textBlock.Inlines.Add(new Run(part));
        }
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

            RenderMatch(_stackContainer.Children, match, null, false);
        }
    }

    private void DoHide()
    {
        Visibility = Visibility.Hidden;

        if (_searchManager != null)
        {
            _searchManager.SearchResultsChanged -= _searchManager_SearchResultsChanged;
            _searchManager.StackChanged -= _searchManager_StackChanged;
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
                {
                    PushItem(searchable, searchResult);
                    e.Handled = true;
                }
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
        var index = _results.SelectedIndex == -1 ? 0 : _results.SelectedIndex + offset;

        if (_results.Items.Count == 0)
            return;

        var item = (ListBoxItem)
            _results.Items[Math.Min(Math.Max(index, 0), _results.Items.Count - 1)];

        item.IsSelected = true;

        _results.ScrollIntoView(item);
    }

    private void SelectPage(int i) => SelectItem(i * 8);

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
}
