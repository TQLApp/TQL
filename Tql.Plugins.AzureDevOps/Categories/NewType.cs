using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class NewType : IMatchType
{
    private readonly AzureWorkItemIconManager _iconManager;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.New.Id;

    public NewType(AzureWorkItemIconManager iconManager, ConfigurationManager configurationManager)
    {
        _iconManager = iconManager;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<NewMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new NewMatch(dto, _iconManager);
    }
}
