using Tql.Abstractions;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class NewsMatch(
    RootItemDto dto,
    ICache<YouTrackData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<NewMatch, NewMatchDto> factory
) : CachedMatch<YouTrackData>(cache), ISerializableMatch
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

    protected override IEnumerable<IMatch> Create(YouTrackData data)
    {
        return from project in data.GetConnection(dto.Url).Projects
            select factory.Create(
                new NewMatchDto(dto.Url, project.ShortName, project.Name, project.IconUrl)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
