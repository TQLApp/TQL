using System.Windows.Forms;
using Tql.App.Services;
using Tql.App.Support;
using Application = System.Windows.Application;

namespace Tql.App;

partial class MainWindow
{
    private readonly NotifyIcon _notifyIcon;

    private NotifyIcon SetupNotifyIcon()
    {
        var notifyIcon = new NotifyIcon();

        notifyIcon.Icon = LoadIcon();
        notifyIcon.Text = "Techie's Quick Launcher";
        notifyIcon.Visible = true;

        notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu();

        var hotKeyMenuItem = notifyIcon.ContextMenu.MenuItems.Add(
            GetHotKeyMenuItemLabel(),
            (_, _) => DoShow()
        );

        _settings.AttachPropertyChanged(
            nameof(_settings.HotKey),
            (_, _) => hotKeyMenuItem.Text = GetHotKeyMenuItemLabel()
        );

        notifyIcon.ContextMenu.MenuItems.Add("-");
#if DEBUG
        notifyIcon.ContextMenu.MenuItems.Add(
            "Invalidate All Caches",
            (_, _) => InvalidateAllCaches()
        );
#endif
        notifyIcon.ContextMenu.MenuItems.Add("S&ettings", (_, _) => OpenSettings());
        notifyIcon.ContextMenu.MenuItems.Add("-");
        notifyIcon.ContextMenu.MenuItems.Add("&Help", (_, _) => OpenHelp());
        notifyIcon.ContextMenu.MenuItems.Add("-");
        notifyIcon.ContextMenu.MenuItems.Add("E&xit", (_, _) => Close());

        notifyIcon.Click += (_, e) => DoShow();

        return notifyIcon;

        System.Drawing.Icon LoadIcon()
        {
            using var stream = Application
                .GetResourceStream(
                    new Uri("pack://application:,,,/Tql.App;component/mainicon.ico")
                )!
                .Stream;

            return new System.Drawing.Icon(stream);
        }
    }

    private string GetHotKeyMenuItemLabel()
    {
        var hotKey = HotKey.FromSettings(_settings);

        var sb = StringBuilderCache.Acquire();

        if (hotKey.Win)
            sb.Append("Win+");
        if (hotKey.Control)
            sb.Append("Ctrl+");
        if (hotKey.Alt)
            sb.Append("Alt+");
        if (hotKey.Shift)
            sb.Append("Shift+");

        sb.Append(HotKey.AvailableKeys.Single(p => p.Key == hotKey.Key).Label);

        return $"&Search\t{StringBuilderCache.GetStringAndRelease(sb)}";
    }

    private void InvalidateAllCaches()
    {
        _cacheManagerManager.InvalidateAllCaches();
    }

    private void RepositionScreen()
    {
        var source = PresentationSource.FromVisual(this);
        if (source == null)
            return;

        var showOnScreen = ShowOnScreenManager.Create(_settings.ShowOnScreen);
        var screen = showOnScreen.GetScreen();

        var scaleX = source.CompositionTarget!.TransformToDevice.M11;
        var scaleY = source.CompositionTarget!.TransformToDevice.M22;

        Left =
            (screen.WorkingArea.Left / scaleX) + ((screen.WorkingArea.Width / scaleX) - Width) / 2;
        Top = (screen.WorkingArea.Top / scaleY) + ((screen.WorkingArea.Height / scaleY) - 30) / 2;
    }
}
