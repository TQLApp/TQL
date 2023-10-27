using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class IssueType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Issue.Id;

    public IssueType(IconCacheManager iconCacheManager, ConfigurationManager configurationManager)
    {
        _iconCacheManager = iconCacheManager;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<IssueMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new IssueMatch(dto, _iconCacheManager);
    }
}
