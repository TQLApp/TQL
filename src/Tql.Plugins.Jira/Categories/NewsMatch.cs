using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class NewsMatch(
    RootItemDto dto,
    ICache<JiraData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<NewMatch, NewMatchDto> factory
) : CachedMatch<JiraData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.NewsMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Issues;
    public override MatchTypeId TypeId => TypeIds.News;
    public override string SearchHint => Labels.NewsMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        foreach (var project in data.GetConnection(dto.Url).Projects)
        {
            yield return factory.Create(
                new NewMatchDto(
                    dto.Url,
                    project.Name,
                    project.Id,
                    NewMatchType.Query,
                    Labels.NewsMatch_Query,
                    null,
                    null
                )
            );

            foreach (var issueType in project.IssueTypes)
            {
                yield return factory.Create(
                    new NewMatchDto(
                        dto.Url,
                        project.Name,
                        project.Id,
                        NewMatchType.Issue,
                        issueType.Name,
                        issueType.Id,
                        issueType.IconUrl
                    )
                );
            }
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
