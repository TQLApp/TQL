using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Support;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Forms.Clipboard;

namespace Tql.App.Services;

internal class UI(ILogger<UI> logger) : IUI
{
    private SynchronizationContext? _synchronizationContext;
    private volatile List<UINotification> _notifications = new();
    private readonly object _syncRoot = new();
    private int _modalDialogShowing;

    public MainWindow? MainWindow { get; private set; }
    public bool IsModalDialogShowing => _modalDialogShowing > 0;

    // This uses the safe publication pattern.
    // ReSharper disable once InconsistentlySynchronizedField
    public ImmutableArray<UINotification> UINotifications => _notifications.ToImmutableArray();

    public event EventHandler? UINotificationsChanged;
    public event EventHandler<ConfigurationUIEventArgs>? ConfigurationUIRequested;

    public void SetSynchronizationContext(SynchronizationContext? synchronizationContext)
    {
        _synchronizationContext = synchronizationContext;
    }

    public Task PerformInteractiveAuthentication(
        IInteractiveAuthentication interactiveAuthentication
    )
    {
        var tcs = new TaskCompletionSource<bool>();

        _synchronizationContext!.Post(
            _ =>
            {
                var window = new InteractiveAuthenticationWindow(interactiveAuthentication, this)
                {
                    Owner = MainWindow
                };

                EnterModalDialog();
                try
                {
                    window.ShowDialog();
                }
                finally
                {
                    ExitModalDialog();
                }

                if (window.Exception != null)
                    tcs.SetException(window.Exception);
                else
                    tcs.SetResult(true);
            },
            null
        );

        return tcs.Task;
    }

    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to open '{Url}'", url);
        }
    }

    public void SetMainWindow(MainWindow? mainWindow)
    {
        MainWindow = mainWindow;
    }

    public void Shutdown(RestartMode mode)
    {
        _synchronizationContext?.Post(
            _ =>
            {
                App.RestartMode = mode;
                Application.Current.Shutdown();
            },
            null
        );
    }

    public DialogResult ShowTaskDialog(
        IWin32Window owner,
        string title,
        string? subtitle = null,
        DialogCommonButtons buttons = DialogCommonButtons.Yes | DialogCommonButtons.No,
        DialogIcon icon = DialogIcon.Warning
    )
    {
        EnterModalDialog();

        try
        {
            var page = new TaskDialogPage
            {
                Caption = Labels.ApplicationTitle,
                Heading = title,
                Text = subtitle,
                AllowCancel =
                    buttons.HasFlag(DialogCommonButtons.Cancel)
                    || buttons.HasFlag(DialogCommonButtons.Close),
                AllowMinimize = false
            };

            TranslateButtons(page, buttons);
            TranslateIcon(page, icon);

            var button = TaskDialog.ShowDialog(owner, page);

            return TranslateResult(button);
        }
        finally
        {
            ExitModalDialog();
        }
    }

    public DialogResult ShowException(
        IWin32Window owner,
        string title,
        Exception exception,
        DialogCommonButtons buttons = DialogCommonButtons.OK,
        DialogIcon icon = DialogIcon.Error
    )
    {
        EnterModalDialog();

        try
        {
            var exceptionInformation = exception.PrintStackTrace();

            var page = new TaskDialogPage
            {
                Caption = Labels.ApplicationTitle,
                Heading = title,
                Text = exception.Message,
                AllowCancel =
                    buttons.HasFlag(DialogCommonButtons.Cancel)
                    || buttons.HasFlag(DialogCommonButtons.Close),
                AllowMinimize = false,
                Expander = new TaskDialogExpander(exceptionInformation)
                {
                    ExpandedButtonText = Labels.Alert_ShowExceptionDetails
                }
            };

            TranslateButtons(page, buttons);
            TranslateIcon(page, icon);

            var copyToClipboard = new TaskDialogButton(Labels.Alert_CopyToClipboard);
            page.Buttons.Add(copyToClipboard);

            var button = TaskDialog.ShowDialog(owner, page);

            if (button == copyToClipboard)
            {
                Clipboard.SetText(exceptionInformation);
                return DialogResult.OK;
            }

            return TranslateResult(button);
        }
        finally
        {
            ExitModalDialog();
        }
    }

    private static void TranslateIcon(TaskDialogPage page, DialogIcon icon)
    {
        page.Icon = icon switch
        {
            DialogIcon.Warning => TaskDialogIcon.Warning,
            DialogIcon.Error => TaskDialogIcon.Error,
            DialogIcon.Information => TaskDialogIcon.Information,
            DialogIcon.Shield => TaskDialogIcon.Shield,
            _ => null
        };
    }

    private static DialogResult TranslateResult(TaskDialogButton button)
    {
        if (button == TaskDialogButton.OK)
            return DialogResult.OK;
        if (button == TaskDialogButton.Yes)
            return DialogResult.Yes;
        if (button == TaskDialogButton.No)
            return DialogResult.No;
        if (button == TaskDialogButton.Cancel)
            return DialogResult.Cancel;
        if (button == TaskDialogButton.Retry)
            return DialogResult.Retry;
        if (button == TaskDialogButton.Close)
            return DialogResult.Cancel;

        throw new InvalidOperationException("Could not map task dialog button");
    }

    private static void TranslateButtons(TaskDialogPage page, DialogCommonButtons buttons)
    {
        if (buttons.HasFlag(DialogCommonButtons.OK))
            page.Buttons.Add(TaskDialogButton.OK);
        if (buttons.HasFlag(DialogCommonButtons.Yes))
            page.Buttons.Add(TaskDialogButton.Yes);
        if (buttons.HasFlag(DialogCommonButtons.No))
            page.Buttons.Add(TaskDialogButton.No);
        if (buttons.HasFlag(DialogCommonButtons.Cancel))
            page.Buttons.Add(TaskDialogButton.Cancel);
        if (buttons.HasFlag(DialogCommonButtons.Retry))
            page.Buttons.Add(TaskDialogButton.Retry);
        if (buttons.HasFlag(DialogCommonButtons.Close))
            page.Buttons.Add(TaskDialogButton.Close);
    }

    public void ShowNotificationBar(
        string key,
        string message,
        Action? activate = null,
        Action? dismiss = null
    )
    {
        lock (_syncRoot)
        {
            var notifications = _notifications.ToList();

            notifications.RemoveAll(p => p.Key == key);
            notifications.Add(new UINotification(key, message, activate, dismiss));

            _notifications = notifications;
        }

        _synchronizationContext?.Post(_ => OnUINotificationsChanged(), null);
    }

    public void RemoveNotificationBar(string key)
    {
        lock (_syncRoot)
        {
            var notifications = _notifications.ToList();

            if (notifications.RemoveAll(p => p.Key == key) == 0)
                return;

            _notifications = notifications;
        }

        _synchronizationContext?.Post(_ => OnUINotificationsChanged(), null);
    }

    public void OpenConfiguration(Guid id)
    {
        OnConfigurationUIRequested(new ConfigurationUIEventArgs(id));
    }

    public void EnterModalDialog()
    {
        if (_modalDialogShowing == 0 && MainWindow != null)
            MainWindow.ShowInTaskbar = true;
        _modalDialogShowing++;
    }

    public void ExitModalDialog()
    {
        _modalDialogShowing--;
        if (_modalDialogShowing == 0 && MainWindow != null)
            MainWindow.ShowInTaskbar = false;
    }

    protected virtual void OnUINotificationsChanged() =>
        UINotificationsChanged?.Invoke(this, EventArgs.Empty);

    protected virtual void OnConfigurationUIRequested(ConfigurationUIEventArgs e) =>
        ConfigurationUIRequested?.Invoke(this, e);
}
