using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph.Client;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Support;

namespace Tql.Plugins.AzureDevOps.Services;

internal class AzureDevOpsPeopleDirectory : IPeopleDirectory
{
    private readonly Connection _connection;
    private readonly AzureDevOpsApi _api;

    public string Id { get; }
    public string Name { get; }

    public AzureDevOpsPeopleDirectory(Connection connection, AzureDevOpsApi api)
    {
        Id = Encryption.Hash($"{AzureDevOpsPlugin.Id}\\{connection.Url}");
        Name = $"Azure DevOps - {connection.Name}";

        _connection = connection;
        _api = api;
    }

    public async Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    )
    {
        if (search.IsWhiteSpace())
            return ImmutableArray<IPerson>.Empty;

        var client = await _api.GetClient<GraphHttpClient>(_connection.Url);

        var users = await client.QuerySubjectsAsync(
            new GraphSubjectQuery(search, new[] { "User" }, new SubjectDescriptor()),
            cancellationToken: cancellationToken
        );

        if (users == null)
            return ImmutableArray<IPerson>.Empty;

        return users
            .Cast<GraphUser>()
            .Select(p => new Person(p.DisplayName, p.MailAddress))
            .ToImmutableArray<IPerson>();
    }

    private record Person(string DisplayName, string EmailAddress) : IPerson;
}
