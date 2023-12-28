using System.Collections.Specialized;
using System.Windows.Forms;

namespace Tql.Abstractions;

/// <summary>
/// Represents the service for interacting with the UI.
/// </summary>
public interface IUI
{
    /// <summary>
    /// Indicates the restart mode of the application.
    /// </summary>
    /// <remarks>
    /// The restart mode indicates what happens when the app
    /// shuts down. This is used e.g. to automatically restart the
    /// app on automatic updates. You can use this to decide if
    /// you want to do cleanup or maintenance actions on application
    /// shutdown. See also <see cref="ILifecycleService"/>.
    /// </remarks>
    RestartMode RestartMode { get; }

    /// <summary>
    /// Perform interactive authentication.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method can be used to perform interactive authentication.
    /// A dialog will be shown to the user telling him that he needs
    /// to enter credentials. The information in <paramref name="resource"/>
    /// is used to tell the user what resource is being authenticated.
    /// You should include the name of a connection in this to ensure
    /// the user knows what credentials he needs to enter. If the
    /// user proceeds to authenticate, the <paramref name="action"/>
    /// parameter is used to show UI to the user.
    /// </para>
    ///
    /// <para>
    /// If you're using a library to talk to the online resource,
    /// there's a good chance the interactive authentication functionality
    /// takes a <see cref="IWin32Window"/> as the owner. However, if you're
    /// creating your own WPF UI, you should use <see cref="System.Windows.Interop.WindowInteropHelper.Owner"/>
    /// to set the owner of your window to the provided handle.
    /// </para>
    /// </remarks>
    /// <param name="resource">Information about the resource being authenticated.</param>
    /// <param name="action">Action called to authenticate the resource.</param>
    /// <returns>Task representing the operation.</returns>
    Task PerformInteractiveAuthentication(
        InteractiveAuthenticationResource resource,
        Func<IWin32Window, Task> action
    );

    /// <summary>
    /// Perform browser based interactive authentication.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This can be used to implement web based interactive authentication workflows
    /// while staying in the app. The redirect URL is the URL configured with the authentication
    /// provider. TQL will start a web server at that URL and wait for an incoming request.
    /// The result of this method includes the parameters sent when the redirect URL
    /// was called.
    /// </para>
    ///
    /// <para>
    /// The OAuth2 NuGet package is a good option to implement OAuth2 in a TQL plugin. This
    /// will provide you with a login URL you can pass into this method.
    /// </para>
    /// </remarks>
    /// <param name="resource">Information about the resource being authenticated.</param>
    /// <param name="loginUrl">The URL to load in the browser.</param>
    /// <param name="redirectUrl">The URL that's expected to be called.</param>
    /// <returns>The parameters of the redirect URI request or an exception on failure.</returns>
    Task<BrowserBasedInteractiveAuthenticationResult> PerformBrowserBasedInteractiveAuthentication(
        InteractiveAuthenticationResource resource,
        string loginUrl,
        string redirectUrl
    );

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
    /// <param name="mode">Mode used when shutting down the app.</param>
    void Shutdown(RestartMode mode);
}

/// <summary>
/// Describes a resource (e.g. a connection) for interactive authentication.
/// </summary>
/// <param name="PluginId">ID of the plugin that the resource belongs to.</param>
/// <param name="ResourceId">Unique ID of the resource.</param>
/// <param name="ResourceName">Name of the resource.</param>
/// <param name="ResourceIcon">Icon of the resource.</param>
public record InteractiveAuthenticationResource(
    Guid PluginId,
    Guid ResourceId,
    string ResourceName,
    ImageSource ResourceIcon
);

/// <summary>
/// Result of a browser based interactive authentication request.
/// </summary>
/// <param name="Url">The called URL.</param>
/// <param name="QueryString">The parsed parameters of the called URI.</param>
public record BrowserBasedInteractiveAuthenticationResult(Uri Url, NameValueCollection QueryString);
