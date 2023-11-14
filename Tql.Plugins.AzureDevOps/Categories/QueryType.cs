using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class QueryType : MatchType<QueryMatch, QueryMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Query.Id;

    public QueryType(
        IMatchFactory<QueryMatch, QueryMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(QueryMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
