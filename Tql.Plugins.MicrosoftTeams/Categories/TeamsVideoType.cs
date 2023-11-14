using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsVideoType : MatchType<TeamsVideoMatch, PersonDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.TeamsVideo.Id;

    public TeamsVideoType(
        IMatchFactory<TeamsVideoMatch, PersonDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(PersonDto dto) =>
        _configurationManager.HasDirectory(dto.DirectoryId);
}
