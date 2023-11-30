using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class NewsMatch(
    RootItemDto dto,
    ICache<AzureData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<NewMatch, NewMatchDto> factory
) : CachedMatch<AzureData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.NewsMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.News;
    public override string SearchHint => Labels.NewsMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        foreach (var project in data.GetConnection(dto.Url).Projects)
        {
            yield return factory.Create(
                new NewMatchDto(dto.Url, project.Name, NewMatchType.Query, null)
            );

            foreach (var workItemType in project.WorkItemTypes)
            {
                yield return factory.Create(
                    new NewMatchDto(dto.Url, project.Name, NewMatchType.WorkItem, workItemType.Name)
                );
            }
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
