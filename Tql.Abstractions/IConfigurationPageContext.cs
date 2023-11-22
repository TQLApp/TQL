namespace Tql.Abstractions;

/// <summary>
/// Represents the context of a configuration page.
/// </summary>
/// <remarks>
/// The configuration page context is a way to interact with TQL.
/// An implementation of this interface is provided to you when
/// your configuration page is initialized. See <see cref="IConfigurationPage.Initialize(IConfigurationPageContext)"/>.
/// </remarks>
public interface IConfigurationPageContext
{
    /// <summary>
    /// Gets whether the configuration page is currently visible.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Occurs when the visibility of the configuration page has changed.
    /// </summary>
    /// <remarks>
    /// Use this event to e.g. delay loading the configuration page
    /// until the user navigates to it.
    /// </remarks>
    event EventHandler? IsVisibleChanged;

    /// <summary>
    /// Occurs after all configuration pages have been saved successfully
    /// and the configuration window is being closed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The intended use case of this event is to request the user to restart
    /// the app if he made any changes that cannot be applied immediately.
    /// See <see cref="IUI.Shutdown(RestartMode)"/> for how to restart the app.
    /// </para>
    ///
    /// <para>
    /// If you want to show a task dialog from this event, you need to be
    /// aware that your page likely isn't currently being shown. However,
    /// the <c>sender</c> parameter of the event will be the configuration
    /// <see cref="Window"/> instance. You can use the <c>sender</c> as the
    /// owner.
    /// </para>
    /// </remarks>
    event EventHandler? Closed;
}
