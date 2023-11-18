using System.Windows.Interop;
using Tql.Abstractions;
using Tql.App.Services;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace Tql.Utilities;

public static class UIExtensions
{
    public static DialogResult ShowConfirmation(
        this IUI self,
        UIElement owner,
        string title,
        string? subtitle = null,
        DialogCommonButtons buttons = DialogCommonButtons.Yes | DialogCommonButtons.No,
        DialogIcon icon = DialogIcon.Warning
    )
    {
        return self.ShowConfirmation(GetOwner(owner), title, subtitle, buttons, icon);
    }

    public static DialogResult ShowError(
        this IUI self,
        UIElement owner,
        string title,
        Exception exception,
        DialogIcon icon = DialogIcon.Error,
        DialogCommonButtons buttons = DialogCommonButtons.OK
    )
    {
        return self.ShowError(GetOwner(owner), title, exception, icon, buttons);
    }

    private static Win32Window GetOwner(UIElement owner)
    {
        return new Win32Window(new WindowInteropHelper(Window.GetWindow(owner)!).Handle);
    }

    internal record Win32Window(IntPtr Handle) : IWin32Window;
}
