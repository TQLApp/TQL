using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Categories;

internal class PortalType : IMatchType
{
    public Guid Id => TypeIds.Portal.Id;

    public IMatch Deserialize(string json)
    {
        return new PortalMatch(JsonSerializer.Deserialize<PortalMatchDto>(json)!);
    }
}
