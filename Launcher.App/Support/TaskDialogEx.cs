using System.Windows.Forms;
using System.Windows.Interop;

namespace Launcher.App.Support;

internal static class TaskDialogEx
{
    public static TaskDialogButton Show(UIElement owner, TaskDialogPage page)
    {
        var interop = new WindowInteropHelper(Window.GetWindow(owner)!);
        var mainWindowSrc = HwndSource.FromHwnd(interop.Handle)!;

        page.AllowMinimize = false;
        page.Caption = "Launcher";

        return TaskDialog.ShowDialog(mainWindowSrc.Handle, page);
    }

    public static void Alert(
        UIElement owner,
        string heading,
        string? text = null,
        TaskDialogIcon? icon = null
    )
    {
        Show(
            owner,
            new TaskDialogPage
            {
                Buttons = new TaskDialogButtonCollection { new TaskDialogButton("OK") },
                Heading = heading,
                Text = text,
                Icon = icon ?? TaskDialogIcon.Error,
                AllowCancel = true
            }
        );
    }

    public static bool Confirm(UIElement owner, string heading, string? text = null)
    {
        var yes = new TaskDialogButton("Yes");

        var result = Show(
            owner,
            new TaskDialogPage
            {
                Buttons = new TaskDialogButtonCollection { yes, new TaskDialogButton("No") },
                Heading = heading,
                Text = text,
                Icon = TaskDialogIcon.Error,
                AllowCancel = true
            }
        );

        return result == yes;
    }
}
