using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

[RootMatchType]
internal class SearchesType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Searches.Id;

    public SearchesType(ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new SearchesMatch(
            MatchUtils.GetMatchLabel("Confluence Search", configuration, dto.Url),
            dto.Url,
            _configurationManager
        );
    }
}
