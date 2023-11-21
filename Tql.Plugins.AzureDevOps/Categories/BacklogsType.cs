using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class BacklogsType(
    IMatchFactory<BacklogsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<BacklogsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Backlogs.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
