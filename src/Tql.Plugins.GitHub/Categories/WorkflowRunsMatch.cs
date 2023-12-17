using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class WorkflowRunsMatch(
    RepositoryItemMatchDto dto,
    GitHubApi api,
    WorkflowRunType workflowRunType,
    IMatchFactory<WorkflowRunMatch, WorkflowRunMatchDto> factory
) : IRunnableMatch, ICopyableMatch, ISearchableMatch, ISerializableMatch
{
    public string Text =>
        MatchText.Path($"{dto.Owner}/{dto.RepositoryName}", Labels.WorkflowRunsMatch_Label);
    public ImageSource Icon => Images.Workflow;
    public MatchTypeId TypeId => TypeIds.WorkflowRuns;
    public string SearchHint => Labels.WorkflowRunsMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(GetUrl());

        return Task.CompletedTask;
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, GetUrl());

        return Task.CompletedTask;
    }

    private string GetUrl() => $"{dto.GetUrl()}/actions";

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var task = context.GetDataCached(
            $"{GetType().FullName}|{dto.Owner}|{dto.RepositoryName}",
            _ => GetWorkflows()
        );

        if (!task.IsCompleted)
            await context.DebounceDelay(cancellationToken);

        var matches = await task;

        if (text.IsWhiteSpace())
            return matches;

        return context.Filter(matches);
    }

    private async Task<ImmutableArray<WorkflowRunMatch>> GetWorkflows()
    {
        var client = await api.GetClient(dto.ConnectionId);

        var options = new ApiOptions
        {
            StartPage = 1,
            PageCount = 1,
            PageSize = 100
        };

        var response = await client.Actions.Workflows.Runs.List(
            dto.Owner,
            dto.RepositoryName,
            new WorkflowRunsRequest(),
            options
        );

        var dtos = (
            from run in response.WorkflowRuns
            select new WorkflowRunMatchDto(
                dto.ConnectionId,
                dto.Owner,
                dto.RepositoryName,
                run.Name,
                run.RunNumber,
                run.DisplayTitle,
                run.Status.Value switch
                {
                    WorkflowRunStatus.Requested
                    or WorkflowRunStatus.Queued
                    or WorkflowRunStatus.Pending
                        => WorkflowRunMatchStatus.Queued,
                    WorkflowRunStatus.InProgress => WorkflowRunMatchStatus.InProgress,
                    WorkflowRunStatus.Completed
                        => run.Conclusion?.Value switch
                        {
                            WorkflowRunConclusion.Success => WorkflowRunMatchStatus.Success,
                            WorkflowRunConclusion.Failure => WorkflowRunMatchStatus.Failure,
                            WorkflowRunConclusion.Cancelled => WorkflowRunMatchStatus.Cancelled,
                            _ => WorkflowRunMatchStatus.Unknown
                        },
                    _ => throw new ArgumentOutOfRangeException()
                },
                run.HtmlUrl
            )
        ).ToList();

        workflowRunType.UpdateCache(dtos);

        return dtos.Select(factory.Create).ToImmutableArray();
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
