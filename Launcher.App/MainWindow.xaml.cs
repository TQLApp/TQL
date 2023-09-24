using System.Windows.Input;
using Launcher.App.Interop;
using Launcher.App.Services;
using Keys = System.Windows.Forms.Keys;
using Screen = System.Windows.Forms.Screen;

namespace Launcher.App;

internal partial class MainWindow
{
    private readonly Settings _settings;
    private readonly IPluginManager _pluginManager;
    private KeyboardHook? _keyboardHook;

    public MainWindow(Settings settings, IPluginManager pluginManager)
    {
        _settings = settings;
        _pluginManager = pluginManager;

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
        var screen = Screen.PrimaryScreen!;

        foreach (var item in Screen.AllScreens)
        {
            if (item.DeviceName == _settings.ShowOnScreen)
                screen = item;
        }

        var source = PresentationSource.FromVisual(this)!;
        var scaleX = source.CompositionTarget!.TransformToDevice.M11;
        var scaleY = source.CompositionTarget!.TransformToDevice.M22;

        Left =
            (screen.WorkingArea.Left / scaleX) + ((screen.WorkingArea.Width / scaleX) - Width) / 2;
        Top = (screen.WorkingArea.Top / scaleY) + ((screen.WorkingArea.Height / scaleY) - 30) / 2;

        Visibility = Visibility.Visible;

        _search.Text = "";
        _search.Focus();
    }

    private void DoHide()
    {
        Visibility = Visibility.Hidden;
    }
}
