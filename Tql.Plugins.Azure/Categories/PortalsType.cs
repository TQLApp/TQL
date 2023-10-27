using Tql.Abstractions;
using Tql.Plugins.Azure.Services;
using Tql.Plugins.Azure.Support;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Categories;

[RootMatchType]
internal class PortalsType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;
    private readonly AzureApi _api;

    public Guid Id => TypeIds.Portals.Id;

    public PortalsType(ConfigurationManager configurationManager, AzureApi api)
    {
        _configurationManager = configurationManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.HasConnection(dto.Id))
            return null;

        return new PortalsMatch(
            MatchUtils.GetMatchLabel("Azure Portal", configuration, dto.Id),
            dto.Id,
            _api
        );
    }
}
