using Tql.App.Services;
using Tql.App.Support;
using ContextMenu = System.Windows.Forms.ContextMenu;

namespace Tql.App;

internal partial class MainWindow
{
    private ContextMenu SetupNotifyIconContextMenu()
    {
        var contextMenu = new ContextMenu();

        var hotKeyMenuItem = contextMenu.MenuItems.Add(
            GetHotKeyMenuItemLabel(),
            (_, _) => DoShow()
        );

        _settings.AttachPropertyChanged(
            nameof(_settings.HotKey),
            (_, _) => hotKeyMenuItem.Text = GetHotKeyMenuItemLabel()
        );

        contextMenu.MenuItems.Add("-");
#if DEBUG
        contextMenu.MenuItems.Add(
            Labels.NotifyMenu_InvalidateAllCachesLabel,
            (_, _) => InvalidateAllCaches()
        );
#endif
        contextMenu.MenuItems.Add(Labels.NotifyMenu_SettingsLabel, (_, _) => OpenSettings());
        contextMenu.MenuItems.Add("-");
        contextMenu.MenuItems.Add(Labels.NotifyMenu_HelpLabel, (_, _) => OpenHelp());
        contextMenu.MenuItems.Add("-");
        contextMenu.MenuItems.Add(Labels.NotifyMenu_ExitLabel, (_, _) => Close());

        return contextMenu;
    }

    private string GetHotKeyMenuItemLabel()
    {
        var hotKey = HotKey.FromSettings(_settings);

        var sb = StringBuilderCache.Acquire();

        if (hotKey.Win)
            sb.Append(Labels.HotKeyWindows).Append('+');
        if (hotKey.Control)
            sb.Append(Labels.HotKeyControl).Append('+');
        if (hotKey.Alt)
            sb.Append(Labels.HotKeyAlt).Append('+');
        if (hotKey.Shift)
            sb.Append(Labels.HotKeyShift).Append('+');

        sb.Append(HotKey.AvailableKeys.Single(p => p.Key == hotKey.Key).Label);

        return $"{Labels.NotifyMenu_SearchLabel}\t{StringBuilderCache.GetStringAndRelease(sb)}";
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
