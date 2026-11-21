using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Services;

internal class YouTrackPeopleDirectory(
    Connection connection,
    YouTrackApi api,
    ILogger<YouTrackPeopleDirectory> logger
) : IPeopleDirectory
{
    public string Id { get; } = Encryption.Sha1Hash($"{YouTrackPlugin.Id}/{connection.Id}");
    public string Name { get; } =
        string.Format(Labels.YouTrackPeopleDirectory_Title, (object)connection.Name);

    public async Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    )
    {
        if (search.IsWhiteSpace())
            return ImmutableArray<IPerson>.Empty;

        var client = api.GetClient(connection);

        try
        {
            // Users without an email address, banned users and guests are
            // left out by the client.
            var users = await client.SearchUsers(
                search.Trim(),
                YouTrackClient.DefaultSearchTop,
                cancellationToken
            );

            return users
                .Select(p => new Person(
                    string.IsNullOrEmpty(p.FullName) ? p.Login : p.FullName,
                    p.Email!
                ))
                .ToImmutableArray<IPerson>();
        }
        catch (YouTrackUnavailableException ex)
        {
            logger.LogWarning(
                ex,
                "Connection '{Name}' may not search users: {Url} answered {StatusCode}",
                connection.Name,
                ex.RequestUrl,
                ex.StatusCode
            );

            return ImmutableArray<IPerson>.Empty;
        }
    }

    private record Person(string DisplayName, string EmailAddress) : IPerson;
}
