using System.Runtime.InteropServices;
using System.Windows.Interop;
using Tql.App.Interop;
using Tql.Utilities;

namespace Tql.App.Support;

public class BlurWindow : BaseWindow
{
    public BlurWindow()
    {
        Background = Brushes.Transparent;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;

        Loaded += BlurWindow_Loaded;
    }

    private void BlurWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var interop = new WindowInteropHelper(this);
        var mainWindowSrc = HwndSource.FromHwnd(interop.Handle);

        // This color is used to blend with the background.
        mainWindowSrc!.CompositionTarget!.BackgroundColor = Color.FromArgb(0x40, 0, 0, 0);

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
}
