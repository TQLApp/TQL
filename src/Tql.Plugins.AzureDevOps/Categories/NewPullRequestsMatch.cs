using System.Diagnostics;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class NewPullRequestsMatch(
    NewPullRequestsMatchDto dto,
    AzureDevOpsApi api,
    IMatchFactory<NewPullRequestMatch, NewPullRequestMatchDto> factory
) : ISerializableMatch, ISearchableMatch
{
    public string Text =>
        MatchText.Path(dto.ProjectName, dto.RepositoryName, Labels.NewPullRequestsMatch_Label);
    public ImageSource Icon => Images.PullRequest;
    public MatchTypeId TypeId => TypeIds.NewPullRequests;
    public string SearchHint => Labels.NewPullRequestsMatch_SearchHint;

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var task = context.GetDataCached(
            $"{GetType()}|{dto.Url}|{dto.RepositoryName}",
            _ => GetMatches()
        );

        if (!task.IsCompleted)
            await context.DebounceDelay(cancellationToken);

        return context.Filter(await task);
    }

    private async Task<ImmutableArray<NewPullRequestMatch>> GetMatches()
    {
        // This works by heuristic. This logic gets all pushes of the current
        // user over the past day, and all pull request the user created
        // over the past day. We use pushes to infer what branches the user
        // created, filter out branches for which we can find a pull request.
        // This is far from perfect, but hopefully is good enough.

        var gitClient = await api.GetClient<GitHttpClient>(dto.Url);
        var userId = await api.GetUserId(dto.Url);
        var fromDate = DateTime.UtcNow - TimeSpan.FromDays(1);

        var pushesTask = gitClient.GetPushesAsync(
            dto.ProjectName,
            dto.RepositoryName,
            searchCriteria: new GitPushSearchCriteria
            {
                FromDate = fromDate,
                PusherId = userId,
                IncludeRefUpdates = true
            }
        );

        var pullRequestsTask = gitClient.GetPullRequestsAsync(
            dto.ProjectName,
            dto.RepositoryName,
            new GitPullRequestSearchCriteria { CreatorId = userId, MinTime = fromDate }
        );

        var pushes = await pushesTask;
        var pullRequests = await pullRequestsTask;

        var pullRequestSourceBranches = pullRequests
            .Select(p => p.SourceRefName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sourceBranches = pushes
            .SelectMany(p => p.RefUpdates)
            .Select(p => p.Name)
            .Distinct()
            .Where(p => !pullRequestSourceBranches.Contains(p));

        return sourceBranches
            .Select(
                p =>
                    factory.Create(
                        new NewPullRequestMatchDto(
                            dto.Url,
                            dto.ProjectName,
                            dto.RepositoryName,
                            GetBranchName(p)
                        )
                    )
            )
            .ToImmutableArray();
    }

    private string GetBranchName(string refName)
    {
        const string prefix = "refs/heads/";

        Debug.Assert(refName.StartsWith(prefix));

        if (refName.StartsWith(prefix))
            return refName.Substring(prefix.Length);

        return refName;
    }
}

internal record NewPullRequestsMatchDto(string Url, string ProjectName, string RepositoryName);
