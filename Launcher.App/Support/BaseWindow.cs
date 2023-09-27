using System.Windows.Media;

namespace Launcher.App.Support;

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
}
