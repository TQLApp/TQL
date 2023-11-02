using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class IssuesType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IconCacheManager _iconCacheManager;

    public Guid Id => TypeIds.Issues.Id;

    public IssuesType(ConfigurationManager configurationManager, IconCacheManager iconCacheManager)
    {
        _configurationManager = configurationManager;
        _iconCacheManager = iconCacheManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.HasConnection(dto.Url))
            return null;

        return new IssuesMatch(
            MatchUtils.GetMatchLabel(Labels.IssuesType_Label, configuration, dto.Url),
            dto.Url,
            _configurationManager,
            _iconCacheManager
        );
    }
}
