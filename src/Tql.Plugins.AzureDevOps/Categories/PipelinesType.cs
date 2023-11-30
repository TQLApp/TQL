using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class PipelinesType(
    IMatchFactory<PipelinesMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<PipelinesMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Pipelines.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
