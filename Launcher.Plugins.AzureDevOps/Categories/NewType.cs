using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class NewType : IMatchType
{
    public Guid Id => TypeIds.New.Id;

    public IMatch Deserialize(string json)
    {
        return new NewMatch(JsonSerializer.Deserialize<NewMatchDto>(json)!);
    }
}
