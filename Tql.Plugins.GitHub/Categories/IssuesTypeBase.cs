using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesTypeBase<TCategory, TMatch>(
    IMatchFactory<TCategory, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<TCategory, RootItemDto>(factory)
    where TCategory : IssuesMatchBase<TMatch>
    where TMatch : IssueMatchBase
{
    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Id);
}
