using System.Runtime.InteropServices;

namespace Tql.App.Support;

public static class Win32Utils
{
    public static (int DpiX, int DpiY) GetTaskbarScreenDpi()
    {
        var taskbarHandle = FindWindow("Shell_TrayWnd", null);

        // taskbarHandle may be zero if the window can't be found. We want
        // the primary monitor then anyway.
        var monitorHandle = MonitorFromWindow(taskbarHandle, MONITOR_DEFAULTTOPRIMARY);

        // Get the DPI
        GetDpiForMonitor(monitorHandle, MDT_EFFECTIVE_DPI, out var dpiX, out var dpiY);

        return ((int)dpiX, (int)dpiY);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("Shcore.dll")]
    private static extern IntPtr GetDpiForMonitor(
        [In] IntPtr hmonitor,
        [In] int dpiType,
        [Out] out uint dpiX,
        [Out] out uint dpiY
    );

    private const uint MONITOR_DEFAULTTOPRIMARY = 1;

    private const int MDT_EFFECTIVE_DPI = 0;
}
