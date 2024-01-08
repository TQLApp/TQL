using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace Tql.App.Support;

public static class WindowInterop
{
    public static Size GetClientSize(IntPtr handle)
    {
        PInvoke.GetClientRect(new HWND(handle), out var clientRect);

        return new Size(clientRect.right - clientRect.left, clientRect.bottom - clientRect.top);
    }

    public static Rectangle GetClientRect(IntPtr handle)
    {
        var clientSize = GetClientSize(handle);

        var rect = new RECT(0, 0, clientSize.Width, clientSize.Height);

        // Get window style
        var style = PInvoke.GetWindowLong(new HWND(handle), WINDOW_LONG_PTR_INDEX.GWL_STYLE);

        // Adjust the rect to include the non-client area
        PInvoke.AdjustWindowRect(ref rect, (WINDOW_STYLE)style, false);

        return new Rectangle(-rect.left, -rect.top, clientSize.Width, clientSize.Height);
    }

    public static void SetTaskbarButtonWindowStyle(Window window, bool visible)
    {
        var handle = new WindowInteropHelper(window).Handle;

        if (visible)
            AddWindowExStyle(handle, WINDOW_EX_STYLE.WS_EX_APPWINDOW);
        else
            RemoveWindowExStyle(handle, WINDOW_EX_STYLE.WS_EX_APPWINDOW);
    }

    public static void SetToolWindowStyle(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;

        AddWindowExStyle(handle, WINDOW_EX_STYLE.WS_EX_TOOLWINDOW);
    }

    private static void AddWindowExStyle(IntPtr handle, WINDOW_EX_STYLE style)
    {
        var extendedStyle = PInvoke.GetWindowLong(
            new HWND(handle),
            WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE
        );
        PInvoke.SetWindowLong(
            new HWND(handle),
            WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
            extendedStyle | (int)style
        );
    }

    private static void RemoveWindowExStyle(IntPtr handle, WINDOW_EX_STYLE style)
    {
        var extendedStyle = PInvoke.GetWindowLong(
            new HWND(handle),
            WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE
        );
        PInvoke.SetWindowLong(
            new HWND(handle),
            WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
            extendedStyle & ~(int)style
        );
    }
}
