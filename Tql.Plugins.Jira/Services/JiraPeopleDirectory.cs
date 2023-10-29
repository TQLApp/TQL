using Tql.Abstractions;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Services;

internal class JiraPeopleDirectory : IPeopleDirectory
{
    private readonly Connection _connection;
    private readonly JiraApi _api;
    public string Id { get; }
    public string Name { get; }

    public JiraPeopleDirectory(Connection connection, JiraApi api)
    {
        _connection = connection;
        _api = api;

        Id = Encryption.Sha1Hash($"{JiraPlugin.Id}/{connection.Id}");
        Name = $"JIRA - {connection.Name}";
    }

    public async Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    )
    {
        if (search.IsWhiteSpace())
            return ImmutableArray<IPerson>.Empty;

        var client = _api.GetClient(_connection);

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
