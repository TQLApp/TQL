using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

internal class ProjectItemMatch(ProjectItemMatchDto dto, ICache<GitHubData> cache)
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch
{
    public string Text
    {
        get
        {
            var sb = StringBuilderCache.Acquire();

            if (dto.RepositoryName != null)
                sb.Append(dto.RepositoryName).Append(' ');
            if (dto.Number.HasValue)
                sb.Append('#').Append(dto.Number.Value).Append(' ');
            sb.Append(dto.Title);

            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }

    public ImageSource Icon =>
        dto.Type switch
        {
            ProjectItemMatchType.Issue
                => dto.State switch
                {
                    ProjectItemState.Open => Images.OpenIssue,
                    ProjectItemState.Closed => Images.ClosedIssue,
                    _ => throw new ArgumentOutOfRangeException()
                },
            ProjectItemMatchType.DraftIssue => Images.DraftIssue,
            ProjectItemMatchType.PullRequest
                => dto.State switch
                {
                    ProjectItemState.Open => Images.OpenPullRequest,
                    ProjectItemState.Closed => Images.ClosedPullRequest,
                    ProjectItemState.Merged => Images.MergedPullRequest,
                    _ => throw new ArgumentOutOfRangeException()
                },
            _ => throw new ArgumentOutOfRangeException()
        };

    public MatchTypeId TypeId => TypeIds.ProjectItem;

    public async Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.GetUrl(await cache.Get()));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public async Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider
            .GetRequiredService<IClipboard>()
            .CopyUri(Text, dto.GetUrl(await cache.Get()));
    }
}

internal record ProjectItemMatchDto(
    Guid ConnectionId,
    string Owner,
    int ProjectNumber,
    int Id,
    string Title,
    ProjectItemMatchType Type,
    string? RepositoryName,
    int? Number,
    ProjectItemState State
)
{
    public string GetUrl(GitHubData cache)
    {
        if (Type == ProjectItemMatchType.PullRequest)
            return $"https://github.com/{Uri.EscapeDataString(Owner)}/{Uri.EscapeDataString(RepositoryName!)}/pull/{Number!.Value}";

        // We return the issue in the pane on the first view. I think that views
        // don't have filters, so this should be fine.

        var connection = cache.GetConnection(ConnectionId);
        var scope = string.Equals(connection.UserName, Owner, StringComparison.OrdinalIgnoreCase)
            ? "users"
            : "orgs";

        return $"https://github.com/{scope}/{Uri.EscapeDataString(Owner)}/projects/{ProjectNumber}/views/1?pane=issue&itemId={Id}";
    }
}

internal enum ProjectItemMatchType
{
    Issue,
    DraftIssue,
    PullRequest
}

internal enum ProjectItemState
{
    Open,
    Closed,
    Merged
}
