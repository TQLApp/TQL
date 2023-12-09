using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class WorkflowRunMatch(WorkflowRunMatchDto dto)
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch
{
    public string Text =>
        MatchText.Path(
            $"{dto.Owner}/{dto.RepositoryName}",
            $"{dto.Name} #{dto.RunNumber}",
            dto.DisplayTitle
        );
    public ImageSource Icon =>
        dto.Status switch
        {
            WorkflowRunMatchStatus.Queued => Images.QueuedWorkflow,
            WorkflowRunMatchStatus.InProgress => Images.InProgressWorkflow,
            WorkflowRunMatchStatus.Success => Images.SucceededWorkflow,
            WorkflowRunMatchStatus.Failure => Images.FailedWorkflow,
            WorkflowRunMatchStatus.Cancelled => Images.CancelledWorkflow,
            _ => Images.Workflow
        };

    public MatchTypeId TypeId => TypeIds.WorkflowRun;

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
        var clipboard = serviceProvider.GetRequiredService<IClipboard>();

        clipboard.CopyMarkdown(Text, dto.Url);

        return Task.CompletedTask;
    }
}

internal record WorkflowRunMatchDto(
    Guid ConnectionId,
    string Owner,
    string RepositoryName,
    string Name,
    long RunNumber,
    string DisplayTitle,
    WorkflowRunMatchStatus Status,
    string Url
);

internal enum WorkflowRunMatchStatus
{
    Unknown,
    Queued,
    InProgress,
    Success,
    Failure,
    Cancelled
}
