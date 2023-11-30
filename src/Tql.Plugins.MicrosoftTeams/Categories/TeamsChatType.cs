using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsChatType(
    IMatchFactory<TeamsChatMatch, PersonDto> factory,
    ConfigurationManager configurationManager
) : MatchType<TeamsChatMatch, PersonDto>(factory)
{
    public override Guid Id => TypeIds.TeamsChat.Id;

    protected override bool IsValid(PersonDto dto) =>
        configurationManager.HasDirectory(dto.DirectoryId);
}
