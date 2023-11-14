using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsVideosMatch : PeopleMatchBase<TeamsVideoMatch>
{
    private readonly RootItemDto _dto;
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;
    private readonly ConfigurationManager _configurationManager;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.TeamsVideosType_Label,
            _configurationManager,
            _peopleDirectoryManager,
            _dto.Id
        );

    public override ImageSource Icon => Images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsVideos;

    public TeamsVideosMatch(
        RootItemDto dto,
        IPeopleDirectoryManager peopleDirectoryManager,
        ConfigurationManager configurationManager,
        IMatchFactory<TeamsVideoMatch, PersonDto> factory
    )
        : base(dto, peopleDirectoryManager, configurationManager, factory)
    {
        _dto = dto;
        _peopleDirectoryManager = peopleDirectoryManager;
        _configurationManager = configurationManager;
    }
}
