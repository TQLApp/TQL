using System.Windows.Forms;

namespace Tql.App.Services;

internal record UINotification(
    string Key,
    string Message,
    Action<IWin32Window>? Activate,
    Action<IWin32Window>? Dismiss
);
