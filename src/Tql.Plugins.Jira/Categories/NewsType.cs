using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class NewsType(
    IMatchFactory<NewsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<NewsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.News.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
