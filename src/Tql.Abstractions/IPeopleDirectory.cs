namespace Tql.Abstractions;

/// <summary>
/// Represents a people directory.
/// </summary>
/// <remarks>
/// See <see cref="IPeopleDirectoryManager"/> for more information.
/// </remarks>
public interface IPeopleDirectory
{
    /// <summary>
    /// Gets the ID of the people directory.
    /// </summary>
    /// <remarks>
    /// This ID is a string instead of a GUID to simplify generating the key.
    /// A straight forward pattern is to e.g. concatenate the plugin ID with
    /// the URL of an online resource of a connection. The default plugins
    /// SHA1 hash these strings, but this is not required.
    /// </remarks>
    string Id { get; }

    /// <summary>
    /// Gets the name of the people directory.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets all people in the directory matching the search string.
    /// </summary>
    /// <remarks>
    /// If the search string is an empty string, the directory is supposed to
    /// return all people in the directory <b>only</b> if this is a fast
    /// operation (presumably because the list of people is cached). Otherwise
    /// it's expected to return an empty list of people.
    /// </remarks>
    /// <param name="search">String to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>People matching the search string.</returns>
    Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    );
}
