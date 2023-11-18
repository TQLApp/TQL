using Tql.App.Services;

namespace Tql.Abstractions;

public interface IUI
{
    Task PerformInteractiveAuthentication(IInteractiveAuthentication interactiveAuthentication);

    void OpenUrl(string url);

    DialogResult ShowConfirmation(
        IWin32Window owner,
        string title,
        string? subtitle = null,
        DialogCommonButtons buttons = DialogCommonButtons.Yes | DialogCommonButtons.No,
        DialogIcon icon = DialogIcon.Warning
    );

    DialogResult ShowError(
        IWin32Window owner,
        string title,
        Exception exception,
        DialogIcon icon = DialogIcon.Error,
        DialogCommonButtons buttons = DialogCommonButtons.OK
    );

    void ShowNotificationBar(
        string key,
        string message,
        Action? activate = null,
        Action? dismiss = null
    );

    void RemoveNotificationBar(string key);

    void OpenConfiguration(Guid id);

    void Shutdown(RestartMode mode);
}
