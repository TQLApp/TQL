using System.Reflection;
using System.Windows.Forms;
using Tql.App.Interop;
using Application = System.Windows.Application;

namespace Tql.App;

partial class MainWindow
{
    private readonly NotifyIcon _notifyIcon;

    private NotifyIcon SetupNotifyIcon()
    {
        var notifyIcon = new NotifyIcon();

        notifyIcon.Icon = LoadIcon();
        notifyIcon.Text = "Launcher";
        notifyIcon.Visible = true;

        notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();

        notifyIcon.ContextMenu.MenuItems.Add("Search\tAlt+Back", (_, _) => DoShow());
        notifyIcon.ContextMenu.MenuItems.Add("-");
#if DEBUG
        notifyIcon.ContextMenu.MenuItems.Add(
            "Invalidate All Caches",
            (_, _) => InvalidateAllCaches()
        );
#endif
        notifyIcon.ContextMenu.MenuItems.Add("Settings", (_, _) => OpenSettings());
        notifyIcon.ContextMenu.MenuItems.Add("-");
        notifyIcon.ContextMenu.MenuItems.Add("Exit", (_, _) => Close());

        notifyIcon.Click += (_, e) =>
        {
            if (
                e is System.Windows.Forms.MouseEventArgs mouseEventArgs
                && mouseEventArgs.Button != MouseButtons.Left
            )
                return;

            notifyIcon
                .GetType()
                .GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(notifyIcon, Array.Empty<object>());
        };
        notifyIcon.DoubleClick += (_, _) => DoShow();

        return notifyIcon;

        System.Drawing.Icon LoadIcon()
        {
            using var stream = Application
                .GetResourceStream(new Uri("/mainicon.ico", UriKind.Relative))!
                .Stream;

            return new System.Drawing.Icon(stream);
        }
    }

    private void InvalidateAllCaches()
    {
        _cacheManagerManager.InvalidateAllCaches();
    }

    private void SetupShortcut()
    {
        _keyboardHook = new KeyboardHook();
        _keyboardHook.RegisterHotKey(ModifierKeys.Alt, Keys.Back);
        _keyboardHook.KeyPressed += _keyboardHook_KeyPressed;
    }

    private void RepositionScreen()
    {
        var screen = Screen.PrimaryScreen!;

        if (
            _settings.ShowOnScreen.HasValue
            && _settings.ShowOnScreen.Value < Screen.AllScreens.Length
        )
        {
            screen = Screen.AllScreens
                .OrderBy(p => p.Bounds.X)
                .ThenBy(p => p.Bounds.Y)
                .Skip(_settings.ShowOnScreen.Value)
                .First();
        }

        var source = PresentationSource.FromVisual(this)!;
        var scaleX = source.CompositionTarget!.TransformToDevice.M11;
        var scaleY = source.CompositionTarget!.TransformToDevice.M22;

        Left =
            (screen.WorkingArea.Left / scaleX) + ((screen.WorkingArea.Width / scaleX) - Width) / 2;
        Top = (screen.WorkingArea.Top / scaleY) + ((screen.WorkingArea.Height / scaleY) - 30) / 2;
    }
}
