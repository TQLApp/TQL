using System.Windows.Forms;
using System.Windows.Interop;
using Launcher.Interop;

namespace Launcher.App.Support;

public static class TaskDialogEx
{
    public static TaskDialog CreateDialog(string title, string? subtitle, TaskDialogIcon mainIcon)
    {
        return new TaskDialog
        {
            CanBeMinimized = false,
            MainIcon = mainIcon,
            MainInstruction = title,
            Content = subtitle,
            PositionRelativeToWindow = true,
            WindowTitle = "Launchers"
        };
    }

    public static DialogResult Confirm(
        UIElement owner,
        string title,
        string? subtitle = null,
        TaskDialogCommonButtons buttons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
        TaskDialogIcon icon = TaskDialogIcon.Warning
    )
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));
        if (title == null)
            throw new ArgumentNullException(nameof(title));

        var taskDialog = CreateDialog(title, subtitle, icon);

        taskDialog.AllowDialogCancellation = (buttons & TaskDialogCommonButtons.Cancel) != 0;
        taskDialog.CommonButtons = buttons;

        return (DialogResult)taskDialog.Show(GetOwner(owner));
    }

    public static void Error(
        UIElement owner,
        string title,
        string? subtitle = null,
        TaskDialogIcon icon = TaskDialogIcon.Error
    )
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));
        if (title == null)
            throw new ArgumentNullException(nameof(title));

        var taskDialog = CreateDialog(title, subtitle, TaskDialogIcon.Error);

        taskDialog.AllowDialogCancellation = true;
        taskDialog.CommonButtons = TaskDialogCommonButtons.OK;

        taskDialog.Show(GetOwner(owner));
    }

    private static IntPtr GetOwner(UIElement owner)
    {
        return new WindowInteropHelper(Window.GetWindow(owner)!).Handle;
    }
}
