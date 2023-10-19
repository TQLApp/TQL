using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using Tql.App.Interop;
using Tql.App.Services;
using Tql.Utilities;

namespace Tql.App.Support;

internal class BlurWindow : BaseWindow
{
    private readonly Settings _settings;
    private Color _tint;

    public BlurWindow(Settings settings)
    {
        _settings = settings;
        Background = Brushes.Transparent;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;

        Loaded += BlurWindow_Loaded;

        _tint = ParseMainWindowTint();

        settings.AttachPropertyChanged(
            nameof(settings.MainWindowTint),
            (_, _) =>
            {
                _tint = ParseMainWindowTint();

                UpdateMainWindowTint();
            }
        );
    }

    private void BlurWindow_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateMainWindowTint();

        var interop = new WindowInteropHelper(this);

        if (Environment.OSVersion.Version.Major >= 6)
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
    }

    private void UpdateMainWindowTint()
    {
        var interop = new WindowInteropHelper(this);
        var mainWindowSrc = HwndSource.FromHwnd(interop.Handle);

        // This color is used to blend with the background.
        mainWindowSrc!.CompositionTarget!.BackgroundColor = _tint;
    }

    private Color ParseMainWindowTint()
    {
        var mainWindowTint = _settings.MainWindowTint;

        return ParseMainWindowTint(mainWindowTint);
    }

    public static Color ParseMainWindowTint(string? value)
    {
        var re = new Regex("^#([a-z0-9]{8})$", RegexOptions.IgnoreCase);

        string tint;
        if (value != null && re.IsMatch(value))
            tint = value;
        else
            tint = Settings.DefaultMainWindowTint;

        var match = re.Match(tint);

        if (!match.Success)
            throw new InvalidOperationException("Default tint is invalid");

        return Color.FromArgb(Parse(0), Parse(2), Parse(4), Parse(6));

        byte Parse(int offset) =>
            (byte)
                int.Parse(
                    match.Groups[1].Value.Substring(offset, 2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture
                );
    }

    public static string PrintMainWindowTint(Color value)
    {
        var sb = StringBuilderCache.Acquire();

        sb.Append('#')
            .Append(value.A.ToString("x2"))
            .Append(value.R.ToString("x2"))
            .Append(value.G.ToString("x2"))
            .Append(value.B.ToString("x2"));

        return StringBuilderCache.GetStringAndRelease(sb);
    }
}
