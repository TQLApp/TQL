using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories.People;

[RootMatchType]
internal class EmailsType : IMatchType
{
    private readonly ConnectionManager _connectionManager;
    private readonly AzureDevOpsApi _api;

    public Guid Id => TypeIds.Emails.Id;

    public EmailsType(ConnectionManager connectionManager, AzureDevOpsApi api)
    {
        _connectionManager = connectionManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new EmailsMatch(
            MatchUtils.GetMatchLabel("Email", _connectionManager, dto.Url),
            dto.Url,
            _api
        );
    }
}
