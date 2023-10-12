using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class BacklogType : IMatchType
{
    public Guid Id => TypeIds.Backlog.Id;

    public IMatch Deserialize(string json)
    {
        return new BacklogMatch(JsonSerializer.Deserialize<BacklogMatchDto>(json)!);
    }
}
