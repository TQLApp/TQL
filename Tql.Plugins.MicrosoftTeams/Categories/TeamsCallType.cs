using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsCallType : IMatchType
{
    public Guid Id => TypeIds.TeamsCall.Id;

    public IMatch Deserialize(string json)
    {
        return new TeamsCallMatch(JsonSerializer.Deserialize<PersonDto>(json)!);
    }
}
