using System.Runtime.InteropServices;
using System.Windows.Interop;
using Tql.App.Interop;
using Tql.Utilities;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;

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
        Background = Brushes.Transparent;

        if (Environment.OSVersion.Version.CompareTo(OSVersions.Windows10) >= 0)
        {
            // This is required for the blur behind to work!
            AllowsTransparency = true;
        }

        SourceInitialized += BlurWindow_SourceInitialized;
    }

    private void BlurWindow_SourceInitialized(object? sender, EventArgs e)
    {
        UpdateMainWindowTint();

        if (Environment.OSVersion.Version.CompareTo(OSVersions.Windows10) >= 0)
        {
            if (!AllowsTransparency)
            {
                throw new InvalidOperationException(
                    "AllowTransparency must be set to true for blur behind to work"
                );
            }

            var interop = new WindowInteropHelper(this);

            Dwm.Windows10EnableBlurBehind(interop.Handle);

            unsafe
            {
                int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                PInvoke.DwmSetWindowAttribute(
                    new HWND(interop.Handle),
                    DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                    &cornerPreference,
                    (uint)Marshal.SizeOf<int>()
                );
            }
        }
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
