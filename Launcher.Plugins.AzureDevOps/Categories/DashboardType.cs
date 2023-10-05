using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class DashboardType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.Dashboard.Id;

    public DashboardType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new DashboardMatch(JsonSerializer.Deserialize<DashboardMatchDto>(json)!, _images);
    }
}
