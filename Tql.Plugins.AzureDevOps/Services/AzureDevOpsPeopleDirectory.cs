using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph.Client;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Services;

internal class AzureDevOpsPeopleDirectory(Connection connection, AzureDevOpsApi api)
    : IPeopleDirectory
{
    public string Id { get; } = Encryption.Sha1Hash($"{AzureDevOpsPlugin.Id}/{connection.Url}");
    public string Name { get; } =
        string.Format(Labels.AzureDevOpsPeopleDirectory_Name, (object)connection.Name);

    public async Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    )
    {
        if (search.IsWhiteSpace())
            return ImmutableArray<IPerson>.Empty;

        var client = await api.GetClient<GraphHttpClient>(connection.Url);

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
