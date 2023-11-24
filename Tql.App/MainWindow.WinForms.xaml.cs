using System.Windows.Forms;
using Tql.App.Services;
using Tql.App.Support;

namespace Tql.App;

internal partial class MainWindow
{
    private ContextMenuStrip SetupNotifyIconContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        var hotKeyMenuItem = (ToolStripMenuItem)
            contextMenu.Items.Add(Labels.NotifyMenu_SearchLabel, null, (_, _) => DoShow());

        hotKeyMenuItem.ShortcutKeyDisplayString = GetHotKeyMenuItemLabel();

        _settings.AttachPropertyChanged(
            nameof(_settings.HotKey),
            (_, _) => hotKeyMenuItem.ShortcutKeyDisplayString = GetHotKeyMenuItemLabel()
        );

        contextMenu.Items.Add("-");
#if DEBUG
        contextMenu
            .Items
            .Add(Labels.NotifyMenu_InvalidateAllCachesLabel, null, (_, _) => InvalidateAllCaches());
#endif
        contextMenu
            .Items
            .Add(Labels.NotifyMenu_ConfigurationLabel, null, (_, _) => OpenConfiguration());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add(Labels.NotifyMenu_HelpLabel, null, (_, _) => OpenHelp());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add(Labels.NotifyMenu_ExitLabel, null, (_, _) => Close());

        return contextMenu;
    }

    private string GetHotKeyMenuItemLabel()
    {
        return HotKey.FromSettings(_settings).ToLabel();
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

        // The window without content is roughly 100 units in height. Most of the time though,
        // it'll have content. Position it slightly above center so that the quick start window
        // has enough space to point at the configuration icon.
        Top = (screen.WorkingArea.Top / scaleY) + ((screen.WorkingArea.Height / scaleY) - 140) / 2;

        // The window height with content is 416 units. Move the window up somewhat if it
        // would not fit in the work area.
        var minTop = (screen.WorkingArea.Bottom / scaleY) - (416 + 10);
        if (Top > minTop)
            Top = minTop;
    }
}
