using System.Runtime.InteropServices;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace Tql.App.Support;

public static class WindowInterop
{
    public static Size GetClientSize(IntPtr handle)
    {
        GetClientRect(handle, out var clientRect);

        return new Size(clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top);
    }

    public static Rectangle GetClientRect(IntPtr handle)
    {
        var clientSize = GetClientSize(handle);

        var rect = new RECT
        {
            Left = 0,
            Top = 0,
            Right = clientSize.Width,
            Bottom = clientSize.Height
        };

        // Get window style
        uint style = GetWindowLong(handle, GWL_STYLE);

        // Adjust the rect to include the non-client area
        AdjustWindowRect(ref rect, style, false);

        return new Rectangle(-rect.Left, -rect.Top, clientSize.Width, clientSize.Height);
    }

    public static void AddWindowStyle(IntPtr handle, uint style)
    {
        var extendedStyle = GetWindowLong(handle, GWL_EXSTYLE);
        SetWindowLong(handle, GWL_EXSTYLE, extendedStyle | style);
    }

    public static void RemoveWindowStyle(IntPtr handle, uint style)
    {
        var extendedStyle = GetWindowLong(handle, GWL_EXSTYLE);
        SetWindowLong(handle, GWL_EXSTYLE, extendedStyle & ~style);
    }

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, uint newStyle);

    [DllImport("user32.dll")]
    private static extern bool AdjustWindowRect(ref RECT lpRect, uint dwStyle, bool bMenu);

    public const int GWL_STYLE = -16;
    public const int GWL_EXSTYLE = -20;

    public const uint WS_EX_TOOLWINDOW = 0x80;
    public const uint WS_EX_APPWINDOW = 0x40000;

    public const int WM_MOUSEACTIVATE = 0x0021;

    public const int MA_NOACTIVATE = 0x0003;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
