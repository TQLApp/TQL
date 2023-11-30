using System.Windows.Forms;

namespace Tql.Abstractions;

/// <summary>
/// Represents the service for interacting with the UI.
/// </summary>
public interface IUI
{
    /// <summary>
    /// Perform interactive authentication.
    /// </summary>
    /// <remarks>
    /// If the tool you're connecting to supports interactive authentication,
    /// you can integrate this into TQL using this method and the
    /// <see cref="IInteractiveAuthentication"/> interface. See the documentation
    /// for this interface for more information.
    /// </remarks>
    /// <param name="interactiveAuthentication">Pending interactive authentication.</param>
    /// <returns>Task representing the operation.</returns>
    Task PerformInteractiveAuthentication(IInteractiveAuthentication interactiveAuthentication);

    /// <summary>
    /// Opens a URL in the default browser.
    /// </summary>
    /// <param name="url">URL to open.</param>
    void OpenUrl(string url);

    /// <summary>
    /// Shows a task dialog to the user.
    /// </summary>
    /// <param name="owner">Owner window of the dialog.</param>
    /// <param name="title">Title shown on the dialog.</param>
    /// <param name="subtitle">Subtitle or descriptive shown on the dialog.</param>
    /// <param name="buttons">Buttons shown on the dialog.</param>
    /// <param name="icon">Icon shown on the dialog.</param>
    /// <returns>Button the user clicked.</returns>
    DialogResult ShowTaskDialog(
        IWin32Window owner,
        string title,
        string? subtitle,
        DialogCommonButtons buttons,
        DialogIcon icon
    );

    /// <summary>
    /// Shows an error dialog to the user with technical information about
    /// the exception.
    /// </summary>
    /// <param name="owner">Owner window of the dialog.</param>
    /// <param name="title">Title shown on the dialog.</param>
    /// <param name="exception">Exception to show to the user.</param>
    /// <param name="buttons">Buttons shown on the dialog (defaults to <see cref="DialogCommonButtons.OK"/>).</param>
    /// <param name="icon">Icon shown on the dialog (defaults to <see cref="DialogIcon.Error"/>).</param>
    /// <returns>Button the user clicked.</returns>
    DialogResult ShowException(
        IWin32Window owner,
        string title,
        Exception exception,
        DialogCommonButtons buttons = DialogCommonButtons.OK,
        DialogIcon icon = DialogIcon.Error
    );

    /// <summary>
    /// Shows a notification bar to the user.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Notification bars appear in the main search window. The purpose of
    /// these is to alert the user of something, e.g. that they need to
    /// refresh their API token if its expired. A general pattern to use
    /// the notification bar is to have a try/catch in your
    /// <see cref="ICacheManager{T}"/> and show the notification bar in
    /// the catch.
    /// </para>
    ///
    /// <para>
    /// The key is used to deduplicate notification bars, and to allow you
    /// to remove them. A general pattern to use is to concatenate your
    /// plugin ID with e.g. the class name where the notification bar
    /// is added.
    /// </para>
    /// </remarks>
    /// <param name="key">Key of the notification bar.</param>
    /// <param name="message">Message to show to the user.</param>
    /// <param name="activate">Action invoked when the notification bar is activated.</param>
    /// <param name="dismiss">Action invoked when the notification bar is dismissed.</param>
    void ShowNotificationBar(
        string key,
        string message,
        Action? activate = null,
        Action? dismiss = null
    );

    /// <summary>
    /// Removes a notification bar.
    /// </summary>
    /// <param name="key">Key of the notification bar to remove.</param>
    void RemoveNotificationBar(string key);

    /// <summary>
    /// Opens the configuration page identified by the ID.
    /// </summary>
    /// <remarks>
    /// This method opens the configuration window with the specified
    /// configuration page selected. This functionality can be used
    /// in conjunction with the notification bar functionality to
    /// open the right configuration page for the user to fix any
    /// issues.
    /// </remarks>
    /// <param name="id">ID of the configuration page to show. See <see cref="IConfigurationPage.PageId"/>.</param>
    void OpenConfiguration(Guid id);

    /// <summary>
    /// Shutdown the app.
    /// </summary>
    /// <remarks>
    /// Call this method e.g. from the <see cref="IConfigurationPageContext.Closed"/>
    /// event to force a restart of the app if the user changes
    /// configuration that requires a restart of the app.
    /// </remarks>
    /// <param name="mode">Mode in which to shutdown the app.</param>
    void Shutdown(RestartMode mode);
}
