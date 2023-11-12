namespace Tql.Interop;

public static class WindowInterop
{
    public static Size GetClientSize(IntPtr handle)
    {
        NativeMethods.GetClientRect(handle, out var clientRect);

        return new Size(clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top);
    }

    public static Rectangle GetClientRect(IntPtr handle)
    {
        var clientSize = GetClientSize(handle);

        var rect = new NativeMethods.RECT
        {
            Left = 0,
            Top = 0,
            Right = clientSize.Width,
            Bottom = clientSize.Height
        };

        // Get window style
        uint style = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE);

        // Adjust the rect to include the non-client area
        NativeMethods.AdjustWindowRect(ref rect, style, false);

        return new Rectangle(-rect.Left, -rect.Top, clientSize.Width, clientSize.Height);
    }

    public static void HideFromTaskSwitcher(IntPtr handle)
    {
        // Change the extended window style to not show in Alt+Tab
        var extendedStyle = NativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(
            handle,
            NativeMethods.GWL_EXSTYLE,
            extendedStyle | NativeMethods.WS_EX_TOOLWINDOW
        );
    }
}
