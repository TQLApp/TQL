using System.Windows.Interop;
using Tql.Abstractions;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace Tql.Utilities;

/// <summary>
/// Utility methods for working with the <see cref="IUI"/> service.
/// </summary>
public static class UIExtensions
{
    /// <summary>
    /// Shows a task dialog to the user.
    /// </summary>
    /// <param name="self">UI service.</param>
    /// <param name="owner">Owner window of the dialog.</param>
    /// <param name="title">Title shown on the dialog.</param>
    /// <param name="subtitle">Subtitle or descriptive shown on the dialog.</param>
    /// <param name="buttons">Buttons shown on the dialog.</param>
    /// <param name="icon">Icon shown on the dialog.</param>
    /// <returns>Button the user clicked.</returns>
    public static DialogResult ShowTaskDialog(
        this IUI self,
        UIElement owner,
        string title,
        string? subtitle,
        DialogCommonButtons buttons,
        DialogIcon icon
    )
    {
        return self.ShowTaskDialog(GetOwner(owner), title, subtitle, buttons, icon);
    }

    /// <summary>
    /// Shows a confirmation dialog to the user.
    /// </summary>
    /// <param name="self">UI service.</param>
    /// <param name="owner">Owner window of the dialog.</param>
    /// <param name="title">Title shown on the dialog.</param>
    /// <param name="subtitle">Subtitle or descriptive shown on the dialog.</param>
    /// <param name="buttons">Buttons shown on the dialog (defaults to <see cref="DialogCommonButtons.Yes"/> and <see cref="DialogCommonButtons.No"/>).</param>
    /// <param name="icon">Icon shown on the dialog (defaults to <see cref="DialogIcon.Warning"/>).</param>
    /// <returns>Button the user clicked.</returns>
    public static DialogResult ShowConfirmation(
        this IUI self,
        UIElement owner,
        string title,
        string? subtitle = null,
        DialogCommonButtons buttons = DialogCommonButtons.Yes | DialogCommonButtons.No,
        DialogIcon icon = DialogIcon.Warning
    )
    {
        return self.ShowTaskDialog(GetOwner(owner), title, subtitle, buttons, icon);
    }

    /// <summary>
    /// Shows an alert dialog to the user.
    /// </summary>
    /// <param name="self">UI service.</param>
    /// <param name="owner">Owner window of the dialog.</param>
    /// <param name="title">Title shown on the dialog.</param>
    /// <param name="subtitle">Subtitle or descriptive shown on the dialog.</param>
    /// <param name="buttons">Buttons shown on the dialog (defaults to <see cref="DialogCommonButtons.OK"/>).</param>
    /// <param name="icon">Icon shown on the dialog (defaults to <see cref="DialogIcon.Error"/>).</param>
    /// <returns>Button the user clicked.</returns>
    public static DialogResult ShowAlert(
        this IUI self,
        UIElement owner,
        string title,
        string? subtitle = null,
        DialogCommonButtons buttons = DialogCommonButtons.OK,
        DialogIcon icon = DialogIcon.Error
    )
    {
        return self.ShowTaskDialog(GetOwner(owner), title, subtitle, buttons, icon);
    }

    /// <summary>
    /// Shows an error dialog to the user with technical information about
    /// the exception.
    /// </summary>
    /// <param name="self">UI service.</param>
    /// <param name="owner">Owner window of the dialog.</param>
    /// <param name="title">Title shown on the dialog.</param>
    /// <param name="exception">Exception to show to the user.</param>
    /// <param name="buttons">Buttons shown on the dialog (defaults to <see cref="DialogCommonButtons.OK"/>).</param>
    /// <param name="icon">Icon shown on the dialog (defaults to <see cref="DialogIcon.Error"/>).</param>
    /// <returns>Button the user clicked.</returns>
    public static DialogResult ShowException(
        this IUI self,
        UIElement owner,
        string title,
        Exception exception,
        DialogCommonButtons buttons = DialogCommonButtons.OK,
        DialogIcon icon = DialogIcon.Error
    )
    {
        return self.ShowException(GetOwner(owner), title, exception, buttons, icon);
    }

    private static Win32Window GetOwner(UIElement owner)
    {
        return new Win32Window(new WindowInteropHelper(Window.GetWindow(owner)!).Handle);
    }

    internal record Win32Window(IntPtr Handle) : IWin32Window;
}
