namespace Launcher.App.Support;

internal record Win32Window(IntPtr Handle)
    : System.Windows.Forms.IWin32Window,
        System.Windows.Interop.IWin32Window;
