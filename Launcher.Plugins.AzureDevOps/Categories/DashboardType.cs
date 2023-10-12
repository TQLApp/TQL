using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class DashboardType : IMatchType
{
    public Guid Id => TypeIds.Dashboard.Id;

    public IMatch Deserialize(string json)
    {
        return new DashboardMatch(JsonSerializer.Deserialize<DashboardMatchDto>(json)!);
    }
}
