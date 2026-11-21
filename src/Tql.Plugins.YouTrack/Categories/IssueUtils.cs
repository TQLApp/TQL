using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;

namespace Tql.Plugins.YouTrack.Categories;

internal static class IssueUtils
{
    public static IEnumerable<IMatch> CreateMatches(
        string url,
        IEnumerable<YouTrackIssueDto> issues,
        IMatchFactory<IssueMatch, IssueMatchDto> factory
    )
    {
        return issues.Select(p => factory.Create(new IssueMatchDto(url, p.IdReadable, p.Summary)));
    }
}
