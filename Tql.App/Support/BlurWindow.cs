using System.Runtime.InteropServices;
using System.Windows.Interop;
using Tql.App.Interop;
using Tql.App.Services;
using Tql.Utilities;

namespace Tql.App.Support;

internal class BlurWindow : BaseWindow
{
    public static readonly DependencyProperty TintProperty = DependencyProperty.Register(
        nameof(Tint),
        typeof(Color),
        typeof(BlurWindow),
        new PropertyMetadata(Colors.Transparent, (d, _) => ((BlurWindow)d).UpdateMainWindowTint())
    );

    public Color Tint
    {
        get => (Color)GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    public BlurWindow()
    {
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;

        SourceInitialized += BlurWindow_SourceInitialized;
    }

    private void BlurWindow_SourceInitialized(object sender, EventArgs e)
    {
        Background = WpfUtils.CreateAcrylicBrush(this);

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
        if (interop.Handle == IntPtr.Zero)
            return;

        var mainWindowSrc = HwndSource.FromHwnd(interop.Handle);
        if (mainWindowSrc == null)
            return;

        // This color is used to blend with the background.
        mainWindowSrc.CompositionTarget!.BackgroundColor = Tint;
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
