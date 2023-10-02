using System.Diagnostics;
using Launcher.App.Interop;
using Launcher.App.Search;
using Launcher.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Keys = System.Windows.Forms.Keys;
using Screen = System.Windows.Forms.Screen;

namespace Launcher.App;

internal partial class MainWindow
{
    private readonly Settings _settings;
    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;
    private KeyboardHook? _keyboardHook;
    private SearchManager? _searchManager;

    public MainWindow(
        Settings settings,
        IPluginManager pluginManager,
        IServiceProvider serviceProvider
    )
    {
        _settings = settings;
        _pluginManager = pluginManager;
        _serviceProvider = serviceProvider;

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
            case Key.Escape:
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

        _searchManager = _serviceProvider.GetRequiredService<SearchManager>();

        Visibility = Visibility.Visible;

        _search.Focus();
    }

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

        _searchManager?.Dispose();
        _searchManager = null;
    }

    private void _search_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchManager?.SearchChanged(_search.Text);
    }
}
