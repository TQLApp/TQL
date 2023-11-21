using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class QueryType(
    IMatchFactory<QueryMatch, QueryMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<QueryMatch, QueryMatchDto>(factory)
{
    public override Guid Id => TypeIds.Query.Id;

    protected override bool IsValid(QueryMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
