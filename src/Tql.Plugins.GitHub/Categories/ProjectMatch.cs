using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class ProjectMatch(
    ProjectMatchDto dto,
    GitHubApi api,
    ICache<GitHubData> cache,
    IMatchFactory<ProjectItemMatch, ProjectItemMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => MatchText.Path(dto.Owner, dto.Title);
    public ImageSource Icon => Images.Project;
    public MatchTypeId TypeId => TypeIds.Project;

    public string SearchHint => Labels.ProjectMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.Url);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.Url);

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var task = context.GetDataCached(
            $"{GetType().FullName}|{dto.Owner}|{dto.Number}",
            _ => GetMatches()
        );

        if (!task.IsCompleted)
            await context.DebounceDelay(cancellationToken);

        return context.Filter(await task);
    }

    private async Task<ImmutableArray<ProjectItemMatch>> GetMatches()
    {
        var data = await cache.Get();

        var connection = data.GetConnection(dto.ConnectionId);
        var graphQlConnection = await api.GetConnection(dto.ConnectionId);

        ProjectV2 projectQuery;
        if (string.Equals(connection.UserName, dto.Owner, StringComparison.OrdinalIgnoreCase))
            projectQuery = new Query().User(dto.Owner).ProjectV2(dto.Number);
        else
            projectQuery = new Query().Organization(dto.Owner).ProjectV2(dto.Number);

        var query = projectQuery
            .Items()
            .AllPages()
            .Select(p =>
                p.Content.Switch<ProjectItem>(p1 =>
                    p1.DraftIssue(p2 => new ProjectItem(
                            p.DatabaseId,
                            p2.Title,
                            ProjectItemMatchType.DraftIssue,
                            null,
                            null,
                            null,
                            null
                        ))
                        .Issue(p2 => new ProjectItem(
                            p.DatabaseId,
                            p2.Title,
                            ProjectItemMatchType.Issue,
                            p2.Repository.Name,
                            p2.Number,
                            p2.State,
                            null
                        ))
                        .PullRequest(p2 => new ProjectItem(
                            p.DatabaseId,
                            p2.Title,
                            ProjectItemMatchType.PullRequest,
                            p2.Repository.Name,
                            p2.Number,
                            null,
                            p2.State
                        ))
                )
            );

        var items = await graphQlConnection.Run(query);

        return items
            .Select(p =>
                factory.Create(
                    new ProjectItemMatchDto(
                        dto.ConnectionId,
                        dto.Owner,
                        dto.Number,
                        p.Id!.Value,
                        p.Title,
                        p.Type,
                        p.RepositoryName,
                        p.Number,
                        GetState(p.IssueState, p.PullRequestState)
                    )
                )
            )
            .ToImmutableArray();
    }

    private ProjectItemState GetState(IssueState? issueState, PullRequestState? pullRequestState)
    {
        if (issueState.HasValue)
        {
            return issueState.Value switch
            {
                IssueState.Open => ProjectItemState.Open,
                IssueState.Closed => ProjectItemState.Closed,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        if (pullRequestState.HasValue)
        {
            return pullRequestState.Value switch
            {
                PullRequestState.Open => ProjectItemState.Open,
                PullRequestState.Closed => ProjectItemState.Closed,
                PullRequestState.Merged => ProjectItemState.Merged,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return ProjectItemState.Open;
    }

    private record ProjectItem(
        int? Id,
        string Title,
        ProjectItemMatchType Type,
        string? RepositoryName,
        int? Number,
        IssueState? IssueState,
        PullRequestState? PullRequestState
    );
}

internal record ProjectMatchDto(
    Guid ConnectionId,
    int Number,
    string Owner,
    string Title,
    string Url
);
