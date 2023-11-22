using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Support;
using Tql.Interop;
using Application = System.Windows.Application;
using TaskDialogIcon = Tql.Interop.TaskDialogIcon;

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

    public void SetSynchronizationContext(SynchronizationContext synchronizationContext)
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
            return TaskDialogEx.Confirm(
                owner,
                title,
                subtitle,
                (TaskDialogCommonButtons)buttons,
                (TaskDialogIcon)icon
            );
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
            return TaskDialogEx.Error(
                owner,
                title,
                exception,
                (TaskDialogIcon)icon,
                (TaskDialogCommonButtons)buttons
            );
        }
        finally
        {
            ExitModalDialog();
        }
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
