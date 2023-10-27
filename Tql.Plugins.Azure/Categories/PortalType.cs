using Tql.Abstractions;
using Tql.Plugins.Azure.Services;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Categories;

internal class PortalType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Portal.Id;

    public PortalType(ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<PortalMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.ConnectionId))
            return null;

        return new PortalMatch(dto);
    }
}
