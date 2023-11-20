using System.Runtime.InteropServices;
using System.Windows.Interop;
using Color = System.Windows.Media.Color;

namespace Tql.Utilities;

/// <summary>
/// Base class for WPF windows.
/// </summary>
/// <remarks>
/// Use this class as the base class for any WPF windows you create to
/// integrate with the TQL theme and styling.
/// </remarks>
public class BaseWindow : Window
{
    /// <summary>
    /// Initializes a new <see cref="BaseWindow"/>.
    /// </summary>
    public BaseWindow()
    {
        TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        ShowInTaskbar = false;
        RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);
        Style = (Style)FindResource("CustomWindowStyle");
    }

    /// <inheritdoc/>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var isBackgroundLight = true;
        if (Background is SolidColorBrush brush)
            isBackgroundLight = IsColorLight(brush.Color);

        if (!isBackgroundLight)
        {
            var interop = new WindowInteropHelper(this);

            var value = 1;
            DwmSetWindowAttribute(
                interop.Handle,
                DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref value,
                sizeof(int)
            );
        }
    }

    // Taken from https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/apply-windows-themes.
    private static bool IsColorLight(Color clr)
    {
        return (((5 * clr.G) + (2 * clr.R) + clr.B) > (8 * 128));
    }

    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        uint attr,
        ref int attrValue,
        int attrSize
    );

    private const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
}
