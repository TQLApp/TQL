using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.Azure.Categories;

internal class PortalType : IMatchType
{
    public Guid Id => TypeIds.Portal.Id;

    public IMatch Deserialize(string json)
    {
        return new PortalMatch(JsonSerializer.Deserialize<PortalMatchDto>(json)!);
    }
}
