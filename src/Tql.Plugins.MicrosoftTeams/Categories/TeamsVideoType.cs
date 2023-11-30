using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsVideoType(
    IMatchFactory<TeamsVideoMatch, PersonDto> factory,
    ConfigurationManager configurationManager
) : MatchType<TeamsVideoMatch, PersonDto>(factory)
{
    public override Guid Id => TypeIds.TeamsVideo.Id;

    protected override bool IsValid(PersonDto dto) =>
        configurationManager.HasDirectory(dto.DirectoryId);
}
