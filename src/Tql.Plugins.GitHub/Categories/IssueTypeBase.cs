using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssueTypeBase<TMatch>(IMatchFactory<TMatch, IssueMatchDto> factory)
    : CachingMatchType<TMatch, IssueMatchDto, IssueMatchDto>(factory)
    where TMatch : IssueMatchBase
{
    protected override IssueMatchDto GetKey(IssueMatchDto dto)
    {
        return dto with { State = 0, Title = "" };
    }
}
