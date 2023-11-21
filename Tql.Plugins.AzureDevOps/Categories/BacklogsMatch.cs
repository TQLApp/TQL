using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BacklogsMatch(
    RootItemDto dto,
    ConfigurationManager configurationManager,
    ICache<AzureData> cache,
    IMatchFactory<BacklogMatch, BacklogMatchDto> factory
) : CachedMatch<AzureData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.BacklogsMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Backlogs;
    public override string SearchHint => Labels.BacklogsMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(dto.Url).Projects
            from team in project.Teams
            from backlog in project.Backlogs
            select factory.Create(
                new BacklogMatchDto(dto.Url, project.Name, team.Name, backlog.Name)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
