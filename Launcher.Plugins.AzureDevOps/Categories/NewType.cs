using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class NewType : IMatchType
{
    private readonly AzureWorkItemIconManager _iconManager;
    public Guid Id => TypeIds.New.Id;

    public NewType(AzureWorkItemIconManager iconManager)
    {
        _iconManager = iconManager;
    }

    public IMatch Deserialize(string json)
    {
        return new NewMatch(JsonSerializer.Deserialize<NewMatchDto>(json)!, _iconManager);
    }
}
