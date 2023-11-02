using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class BacklogsType : IMatchType
{
    private readonly ICache<AzureData> _cache;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Backlogs.Id;

    public BacklogsType(ICache<AzureData> cache, ConfigurationManager configurationManager)
    {
        _cache = cache;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.HasConnection(dto.Url))
            return null;

        return new BacklogsMatch(
            MatchUtils.GetMatchLabel(Labels.BacklogsType_Label, configuration, dto.Url),
            dto.Url,
            _cache
        );
    }
}
