using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesTypeBase<TCategory, TMatch>(
    IMatchFactory<TCategory, RepositoryItemMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<TCategory, RepositoryItemMatchDto>(factory)
    where TCategory : IssuesMatchBase<TMatch>
    where TMatch : IssueMatchBase
{
    protected override bool IsValid(RepositoryItemMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
