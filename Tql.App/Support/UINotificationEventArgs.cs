using Tql.App.Services;

namespace Tql.App.Support;

internal class UINotificationEventArgs : EventArgs
{
    public UINotification Notification { get; }

    public UINotificationEventArgs(UINotification notification)
    {
        Notification = notification;
    }
}
