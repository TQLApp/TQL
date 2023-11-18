namespace Tql.Abstractions;

/// <summary>
/// Represents a service to manage people directories.
/// </summary>
/// <remarks>
/// <para>
/// People directories are services that expose a search function
/// for people directories. The most straight forward example is
/// implemented by the Outlook plugin. This plugin exposes the Global
/// Address List as a people directory. The Microsoft Teams plugin
/// uses this people directory to allow you to start a chat or
/// call people.
/// </para>
///
/// <para>
/// However, other plugins expose their own people directory. E.g. the
/// JIRA plugin exposes a people directory of all JIRA users. This
/// can be useful if a user doesn't have Outlook, or if there are
/// people in the JIRA people directory that are not in the Global Address
/// List.
/// </para>
///
/// <para>
/// People directories are managed by this service. If you want to
/// expose a people directory, implement the <see cref="IPeopleDirectory"/>
/// interface and add it to this service. If you want to use
/// people directories, use the <see cref="Directories"/> property
/// to get all people directories. Note that you should allow the user
/// to select which people directories he wants to use. See the
/// Microsoft Teams plugin for an example.
/// </para>
/// </remarks>
public interface IPeopleDirectoryManager
{
    /// <summary>
    /// Gets all available people directories.
    /// </summary>
    ImmutableArray<IPeopleDirectory> Directories { get; }

    /// <summary>
    /// Adds a people directory.
    /// </summary>
    /// <param name="directory">People directory to add.</param>
    void Add(IPeopleDirectory directory);

    /// <summary>
    /// Removes a people directory.
    /// </summary>
    /// <param name="directory">People directory to remove.</param>
    void Remove(IPeopleDirectory directory);
}
