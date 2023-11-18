using Tql.Abstractions;

namespace Tql.PluginTestSupport.Services;

public class TestUI : IUI
{
    private readonly List<NotificationBarData> _notifications = new();
    private readonly object _syncRoot = new();

    public ImmutableArray<NotificationBarData> NotificationBars
    {
        get
        {
            lock (_syncRoot)
            {
                return _notifications.ToImmutableArray();
            }
        }
    }

    public OpenUrlData? OpenUrlData { get; private set; }

    public Task PerformInteractiveAuthentication(
        IInteractiveAuthentication interactiveAuthentication
    )
    {
        return interactiveAuthentication.Authenticate(new Win32Window(IntPtr.Zero));
    }

    public void OpenUrl(string url) => OpenUrlData = new OpenUrlData(url);

    public void ShowNotificationBar(
        string key,
        string message,
        Action? activate = null,
        Action? dismiss = null
    )
    {
        lock (_syncRoot)
        {
            RemoveNotificationBar(key);

            _notifications.Add(new NotificationBarData(key, message, activate, dismiss));
        }
    }

    public void RemoveNotificationBar(string key)
    {
        lock (_syncRoot)
        {
            _notifications.RemoveAll(p => p.Key == key);
        }
    }

    public void OpenConfiguration(Guid id)
    {
        throw new NotSupportedException();
    }

    public DialogResult ShowException(
        IWin32Window owner,
        string title,
        Exception exception,
        DialogCommonButtons buttons = DialogCommonButtons.OK,
        DialogIcon icon = DialogIcon.Error
    )
    {
        throw new NotSupportedException();
    }

    public DialogResult ShowTaskDialog(
        IWin32Window owner,
        string title,
        string? subtitle = null,
        DialogCommonButtons buttons =
            DialogCommonButtons.None | DialogCommonButtons.Yes | DialogCommonButtons.No,
        DialogIcon icon = DialogIcon.Warning
    )
    {
        throw new NotSupportedException();
    }

    public void Shutdown(RestartMode mode)
    {
        throw new NotSupportedException();
    }

    private record Win32Window(IntPtr Handle) : IWin32Window;
}

public record NotificationBarData(string Key, string Message, Action? Activate, Action? Dismiss);

public record OpenUrlData(string Url);
