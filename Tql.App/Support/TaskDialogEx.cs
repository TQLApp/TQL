using System.Windows.Forms;
using System.Windows.Interop;
using Tql.Interop;
using Clipboard = System.Windows.Forms.Clipboard;

namespace Tql.App.Support;

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
            WindowTitle = "Techie's Quick Launcher"
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
        var taskDialog = CreateDialog(title, subtitle, icon);

        taskDialog.AllowDialogCancellation = (buttons & TaskDialogCommonButtons.Cancel) != 0;
        taskDialog.CommonButtons = buttons;

        return (DialogResult)taskDialog.Show(GetOwner(owner));
    }

    public static DialogResult Error(
        UIElement owner,
        string title,
        Exception exception,
        TaskDialogIcon icon = TaskDialogIcon.Error,
        TaskDialogCommonButtons buttons = TaskDialogCommonButtons.OK
    )
    {
        var exceptionInformation = exception.PrintStackTrace();

        var taskDialog = CreateDialog(title, exception.Message, icon);

        taskDialog.AllowDialogCancellation = true;
        taskDialog.CommonButtons = buttons;
        taskDialog.Width = 300;

        taskDialog.ExpandedControlText = "Show exception details";
        taskDialog.ExpandedInformation = exceptionInformation;

        taskDialog.Buttons = new[] { new TaskDialogButton(101, "Copy to Clipboard") };

        var result = taskDialog.Show(GetOwner(owner));

        if (result == 101)
        {
            Clipboard.SetText(exceptionInformation);
            result = (int)DialogResult.OK;
        }

        return (DialogResult)result;
    }

    private static IntPtr GetOwner(UIElement owner)
    {
        return new WindowInteropHelper(Window.GetWindow(owner)!).Handle;
    }
}
