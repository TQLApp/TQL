using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class DashboardType : IMatchType
{
    public Guid Id => TypeIds.Dashboard.Id;

    public IMatch Deserialize(string json)
    {
        return new DashboardMatch(JsonSerializer.Deserialize<DashboardMatchDto>(json)!);
    }
}
