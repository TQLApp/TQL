using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Services;

internal class JiraPeopleDirectory(Connection connection, JiraApi api) : IPeopleDirectory
{
    public string Id { get; } = Encryption.Sha1Hash($"{JiraPlugin.Id}/{connection.Id}");
    public string Name { get; } =
        string.Format(Labels.JiraPeopleDirectory_Title, (object)connection.Name);

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
            var users = await client.SearchUsersV3(search, 100, cancellationToken);

            return users
                .Where(p => !string.IsNullOrEmpty(p.EmailAddress))
                .Select(p => new Person(p.DisplayName, p.EmailAddress))
                .ToImmutableArray<IPerson>();
        }
        catch when (!cancellationToken.IsCancellationRequested)
        {
            var users = await client.SearchUsers(search, cancellationToken);

            return users
                .Where(p => !string.IsNullOrEmpty(p.EmailAddress))
                .Select(p => new Person(p.DisplayName, p.EmailAddress))
                .ToImmutableArray<IPerson>();
        }
    }

    private record Person(string DisplayName, string EmailAddress) : IPerson;
}
