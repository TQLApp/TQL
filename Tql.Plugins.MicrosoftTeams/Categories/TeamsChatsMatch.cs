using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsChatsMatch : PeopleMatchBase<TeamsChatMatch>
{
    private readonly RootItemDto _dto;
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;
    private readonly ConfigurationManager _configurationManager;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.TeamsChatsMatch_Label,
            _configurationManager,
            _peopleDirectoryManager,
            _dto.Id
        );

    public override ImageSource Icon => Images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsChats;

    public TeamsChatsMatch(
        RootItemDto dto,
        IPeopleDirectoryManager peopleDirectoryManager,
        ConfigurationManager configurationManager,
        IMatchFactory<TeamsChatMatch, PersonDto> factory
    )
        : base(dto, peopleDirectoryManager, configurationManager, factory)
    {
        _dto = dto;
        _peopleDirectoryManager = peopleDirectoryManager;
        _configurationManager = configurationManager;
    }
}
