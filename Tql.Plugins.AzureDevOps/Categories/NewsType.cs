using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

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
