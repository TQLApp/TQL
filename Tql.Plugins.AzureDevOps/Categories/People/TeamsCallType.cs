using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories.People;

internal class TeamsCallType : IMatchType
{
    public Guid Id => TypeIds.TeamsCall.Id;

    public IMatch Deserialize(string json)
    {
        return new TeamsCallMatch(JsonSerializer.Deserialize<GraphUserDto>(json)!);
    }
}
