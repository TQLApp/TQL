using Tql.App.Services.UIService;

namespace Tql.App.Support;

internal class UINotificationEventArgs(UINotification notification) : EventArgs
{
    public UINotification Notification { get; } = notification;
}
