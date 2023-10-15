using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories.People;

internal class EmailType : IMatchType
{
    public Guid Id => TypeIds.Email.Id;

    public IMatch Deserialize(string json)
    {
        return new EmailMatch(JsonSerializer.Deserialize<GraphUserDto>(json)!);
    }
}
