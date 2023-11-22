using System.Windows.Forms;
using Tql.Interop;
using Clipboard = System.Windows.Forms.Clipboard;
using IWin32Window = System.Windows.Forms.IWin32Window;
using TaskDialog = Tql.Interop.TaskDialog;
using TaskDialogButton = Tql.Interop.TaskDialogButton;
using TaskDialogIcon = Tql.Interop.TaskDialogIcon;

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
            WindowTitle = Labels.ApplicationTitle
        };
    }

    public static DialogResult Confirm(
        IWin32Window owner,
        string title,
        string? subtitle = null,
        TaskDialogCommonButtons buttons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
        TaskDialogIcon icon = TaskDialogIcon.Warning
    )
    {
        var taskDialog = CreateDialog(title, subtitle, icon);

        taskDialog.AllowDialogCancellation = (buttons & TaskDialogCommonButtons.Cancel) != 0;
        taskDialog.CommonButtons = buttons;

        return (DialogResult)taskDialog.Show(owner);
    }

    public static DialogResult Error(
        IWin32Window owner,
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

        taskDialog.ExpandedControlText = Labels.Alert_ShowExceptionDetails;
        taskDialog.ExpandedInformation = exceptionInformation;

        taskDialog.Buttons = new[] { new TaskDialogButton(101, Labels.Alert_CopyToClipboard) };

        var result = taskDialog.Show(owner);

        if (result == 101)
        {
            Clipboard.SetText(exceptionInformation);
            result = (int)DialogResult.OK;
        }

        return (DialogResult)result;
    }
}
