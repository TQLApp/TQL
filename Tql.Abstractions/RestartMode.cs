namespace Tql.Abstractions;

/// <summary>
/// Specifies the restart mode used in <see cref="IUI.Shutdown(RestartMode)"/>.
/// </summary>
public enum RestartMode
{
    /// <summary>
    /// Identifies that the app should be shutdown.
    /// </summary>
    Shutdown,

    /// <summary>
    /// Identifies that the app should be restarted. The main window will
    /// be shown to the user after the app has restarted.
    /// </summary>
    Restart,

    /// <summary>
    /// Identifies that the app should be restarted silently.
    /// </summary>
    SilentRestart
}
