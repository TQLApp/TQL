using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;

namespace Tql.Utilities;

public class BaseWindow : Window
{
    public BaseWindow()
    {
        TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);
        Style = (Style)FindResource("CustomWindowStyle");
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var interop = new WindowInteropHelper(this);

        var value = 1;
        DwmSetWindowAttribute(interop.Handle, DWMWA_NCRENDERING_POLICY, ref value, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        uint attr,
        ref int attrValue,
        int attrSize
    );

    private const uint DWMWA_NCRENDERING_POLICY = 20;
}
