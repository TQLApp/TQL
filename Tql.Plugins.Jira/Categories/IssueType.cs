using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class IssueType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;
    private readonly ConnectionManager _connectionManager;
    public Guid Id => TypeIds.Issue.Id;

    public IssueType(IconCacheManager iconCacheManager, ConnectionManager connectionManager)
    {
        _iconCacheManager = iconCacheManager;
        _connectionManager = connectionManager;
    }

    public IMatch Deserialize(string json)
    {
        return new IssueMatch(
            JsonSerializer.Deserialize<IssueMatchDto>(json)!,
            _iconCacheManager,
            _connectionManager
        );
    }
}
