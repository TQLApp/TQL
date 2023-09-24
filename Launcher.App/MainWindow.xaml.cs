using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using Launcher.App.Interop;
using System.Windows.Interop;
using System.Windows.Media;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ModifierKeys = Launcher.App.Interop.ModifierKeys;

namespace Launcher.App;

public partial class MainWindow
{
    private readonly Settings _settings;
    private KeyboardHook? _keyboardHook;

    public MainWindow(Settings settings)
    {
        _settings = settings;
        InitializeComponent();

        SetupShortcut();
    }

    private void SetupShortcut()
    {
        _keyboardHook = new KeyboardHook();
        _keyboardHook.RegisterHotKey(ModifierKeys.Alt, System.Windows.Forms.Keys.Back);
        _keyboardHook.KeyPressed += _keyboardHook_KeyPressed;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var interop = new WindowInteropHelper(this);
        var mainWindowSrc = HwndSource.FromHwnd(interop.Handle);

        mainWindowSrc!.CompositionTarget!.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

        if (Environment.OSVersion.Version.Major > 6)
        {
            Dwm.Windows10EnableBlurBehind(interop.Handle);
            int cornerPreference = (int)Dwm.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
            Dwm.DwmSetWindowAttribute(
                interop.Handle,
                Dwm.DWMWINDOWATTRIBUTE.WindowCornerPreference,
                ref cornerPreference,
                Marshal.SizeOf<int>()
            );
        }
        else
        {
            Dwm.WindowEnableBlurBehind(interop.Handle);
        }

        //// Set Drop shadow of a border-less Form
        //if (WindowStyle == WindowStyle.None)
        //    Dwm.WindowBorderlessDropShadow(interop.Handle, 2);

#if DEBUG
        DoShow();
#endif
    }

    private void _keyboardHook_KeyPressed(object? sender, KeyPressedEventArgs e)
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
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            DoHide();
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
