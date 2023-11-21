using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsCallType(
    IMatchFactory<TeamsCallMatch, PersonDto> factory,
    ConfigurationManager configurationManager
) : MatchType<TeamsCallMatch, PersonDto>(factory)
{
    public override Guid Id => TypeIds.TeamsCall.Id;

    protected override bool IsValid(PersonDto dto) =>
        configurationManager.HasDirectory(dto.DirectoryId);
}
