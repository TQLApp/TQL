using System.Windows;

namespace Tql.Abstractions;

public interface IUI
{
    Task PerformInteractiveAuthentication(IInteractiveAuthentication interactiveAuthentication);

    void OpenUrl(string url);

    DialogResult ShowConfirmation(
        UIElement owner,
        string title,
        string? subtitle = null,
        DialogCommonButtons buttons = DialogCommonButtons.Yes | DialogCommonButtons.No,
        DialogIcon icon = DialogIcon.Warning
    );

    DialogResult ShowError(
        UIElement owner,
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
