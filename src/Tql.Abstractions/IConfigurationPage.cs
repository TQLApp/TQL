namespace Tql.Abstractions;

/// <summary>
/// Represents a configuration page.
/// </summary>
/// <remarks>
/// <para>
/// Configuration pages allow a plugin to expose configuration UI
/// to the user. You can return one or more configuration pages from
/// the <see cref="ITqlPlugin.GetConfigurationPages()"/> method.
/// </para>
///
/// <para>
/// Configuration pages are WPF elements and are presented in a
/// <see cref="ContentPresenter"/>. TQL requires you to implement this
/// interface on these elements so that it can interact with your
/// configuration page.
/// </para>
/// </remarks>
public interface IConfigurationPage
{
    /// <summary>
    /// Gets the ID of the configuration page.
    /// </summary>
    /// <remarks>
    /// The page ID can be used to open a specific configuration page
    /// for the user using the <see cref="IUI.OpenConfiguration(Guid)"/>
    /// method.
    /// </remarks>
    Guid PageId { get; }

    /// <summary>
    /// Gets the title for the configuration page.
    /// </summary>
    /// <remarks>
    /// The title is used to identify the configuration page in the
    /// tree view in the configuration window.
    /// </remarks>
    string Title { get; }

    /// <summary>
    /// Gets the mode in which the configuration page is shown.
    /// </summary>
    ConfigurationPageMode PageMode { get; }

    /// <summary>
    /// Initialize the configuration page.
    /// </summary>
    /// <param name="context">Context to integrate with TQL.</param>
    void Initialize(IConfigurationPageContext context);

    /// <summary>
    /// Saves the configuration if possible.
    /// </summary>
    /// <remarks>
    /// The default behavior of the save method is to just save the
    /// configuration, e.g. using <see cref="IConfigurationManager.SetConfiguration(Guid, string?)"/>.
    /// If the configuration can't be saved, e.g. because the user
    /// didn't enter all fields, this method should itself show a message
    /// to the user and then return <see cref="SaveStatus.Failure"/>.
    /// See <see cref="IUI.ShowException"/>
    /// for a way to show messages to the user.
    /// </remarks>
    /// <returns>Whether the configuration was saved successfully.</returns>
    Task<SaveStatus> Save();
}
