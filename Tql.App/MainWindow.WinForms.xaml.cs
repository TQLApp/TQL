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
        contextMenu.MenuItems.Add(
            Labels.NotifyMenu_ConfigurationLabel,
            (_, _) => OpenConfiguration()
        );
        contextMenu.MenuItems.Add("-");
        contextMenu.MenuItems.Add(Labels.NotifyMenu_HelpLabel, (_, _) => OpenHelp());
        contextMenu.MenuItems.Add("-");
        contextMenu.MenuItems.Add(Labels.NotifyMenu_ExitLabel, (_, _) => Close());

        return contextMenu;
    }

    private string GetHotKeyMenuItemLabel()
    {
        var hotKey = HotKey.FromSettings(_settings);

        return $"{Labels.NotifyMenu_SearchLabel}\t{hotKey.ToLabel()}";
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
        // The window is roughly 100 units in height. Most of the time though, it'll have
        // content. Position it slightly above center so that the quick start window
        // has enough space to point at the configuration icon.
        Top = (screen.WorkingArea.Top / scaleY) + ((screen.WorkingArea.Height / scaleY) - 140) / 2;
    }
}
