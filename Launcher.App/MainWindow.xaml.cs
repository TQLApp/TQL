using Launcher.Abstractions;
using Launcher.App.Interop;
using Launcher.App.Search;
using Launcher.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Image = System.Windows.Controls.Image;
using Keys = System.Windows.Forms.Keys;
using Screen = System.Windows.Forms.Screen;

namespace Launcher.App;

internal partial class MainWindow
{
    private readonly Settings _settings;
    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainWindow> _logger;
    private KeyboardHook? _keyboardHook;
    private SearchManager? _searchManager;

    public MainWindow(
        Settings settings,
        IPluginManager pluginManager,
        IServiceProvider serviceProvider,
        ILogger<MainWindow> logger
    )
    {
        _settings = settings;
        _pluginManager = pluginManager;
        _serviceProvider = serviceProvider;
        _logger = logger;

        InitializeComponent();

        SetupShortcut();
    }

    private void SetupShortcut()
    {
        _keyboardHook = new KeyboardHook();
        _keyboardHook.RegisterHotKey(ModifierKeys.Alt, Keys.Back);
        _keyboardHook.KeyPressed += _keyboardHook_KeyPressed;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
#if DEBUG
        DoShow();

        //var window = _serviceProvider.GetRequiredService<ConfigurationUI.ConfigurationWindow>();
        //window.Owner = this;
        //window.ShowDialog();
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

        Visibility = Visibility.Visible;

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

        var textDecoration = new TextDecoration
        {
            Location = TextDecorationLocation.Underline,
            Pen = new Pen((Brush)_results.FindResource("WavyBrush"), 6)
        };

        foreach (var result in _searchManager.Results)
        {
            var listBoxItem = new ListBoxItem
            {
                FontSize = _search.FontSize,
                VerticalAlignment = VerticalAlignment.Center,
                IsSelected = _results.Items.Count == 0,
                Tag = result
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2)
            };

            listBoxItem.Content = stackPanel;

            stackPanel.Children.Add(
                new Image
                {
                    Source = ((Services.Image)result.Match.Icon).BitmapImage,
                    Width = 18,
                    Margin = new Thickness(0, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center
                }
            );

            var textBlock = new TextBlock();

            stackPanel.Children.Add(textBlock);

            var offset = 0;

            var textMatch = result.TextMatch;
            var text = result.Text;

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

                        if (result.IsFuzzyMatch)
                            inline.TextDecorations.Add(textDecoration);

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

            _results.Items.Add(listBoxItem);
        }
    }

    private void _searchManager_StackChanged(object sender, EventArgs e) { }

    private void RepositionScreen()
    {
        var screen = Screen.PrimaryScreen!;

        if (
            _settings.ShowOnScreen.HasValue
            && _settings.ShowOnScreen.Value < Screen.AllScreens.Length
        )
        {
            screen = Screen.AllScreens
                .OrderBy(p => p.Bounds.X)
                .ThenBy(p => p.Bounds.Y)
                .Skip(_settings.ShowOnScreen.Value)
                .First();
        }

        var source = PresentationSource.FromVisual(this)!;
        var scaleX = source.CompositionTarget!.TransformToDevice.M11;
        var scaleY = source.CompositionTarget!.TransformToDevice.M22;

        Left =
            (screen.WorkingArea.Left / scaleX) + ((screen.WorkingArea.Width / scaleX) - Width) / 2;
        Top = (screen.WorkingArea.Top / scaleY) + ((screen.WorkingArea.Height / scaleY) - 30) / 2;
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
        var selected = default(IMatch);

        if (_results.SelectedItem != null)
        {
            var listBoxItem = (ListBoxItem)_results.SelectedItem;
            var searchResult = (SearchResult)listBoxItem.Tag;
            selected = searchResult.Match;
        }

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
                if (selected is IRunnableMatch runnable)
                {
                    RunItem(runnable);
                    e.Handled = true;
                }
                break;

            case Key.Tab:
                if (selected is ISearchableMatch searchable)
                {
                    PushItem(searchable);
                    e.Handled = true;
                }
                break;

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

    private async void RunItem(IRunnableMatch match)
    {
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

    private void PushItem(ISearchableMatch match)
    {
        _search.Text = string.Empty;

        _results.Items.Clear();

        _searchManager?.Push(match);
    }

    private void PopItem()
    {
        _search.Text = string.Empty;

        _results.Items.Clear();

        _searchManager?.Pop();
    }
}
