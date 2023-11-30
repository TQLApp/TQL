using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class QueriesType(
    IMatchFactory<QueriesMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<QueriesMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Queries.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
