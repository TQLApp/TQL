namespace Tql.Interop;

public static class Win32Utils
{
    public static (int DpiX, int DpiY) GetTaskbarScreenDpi()
    {
        var taskbarHandle = NativeMethods.FindWindow("Shell_TrayWnd", null);

        // taskbarHandle may be zero if the window can't be found. We want
        // the primary monitor then anyway.
        var monitorHandle = NativeMethods.MonitorFromWindow(
            taskbarHandle,
            NativeMethods.MONITOR_DEFAULTTOPRIMARY
        );

        // Get the DPI
        NativeMethods.GetDpiForMonitor(
            monitorHandle,
            NativeMethods.MDT_EFFECTIVE_DPI,
            out var dpiX,
            out var dpiY
        );

        return ((int)dpiX, (int)dpiY);
    }
}
