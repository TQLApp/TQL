using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Categories;

internal class RepositoryMatch(
    RepositoryMatchDto dto,
    IMatchFactory<IssuesMatch, RepositoryItemMatchDto> issuesFactory,
    IMatchFactory<PullRequestsMatch, RepositoryItemMatchDto> pullRequestsFactory,
    IMatchFactory<MilestonesMatch, RepositoryItemMatchDto> milestonesFactory,
    IMatchFactory<WorkflowRunsMatch, RepositoryItemMatchDto> workflowRunsFactory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => $"{dto.Owner}/{dto.RepositoryName}";
    public ImageSource Icon => Images.Repository;
    public MatchTypeId TypeId => TypeIds.Repository;

    public string SearchHint => Labels.RepositoryMatch_SearchHint;

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

    public Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var itemDto = new RepositoryItemMatchDto(dto.ConnectionId, dto.Owner, dto.RepositoryName);

        return Task.FromResult<IEnumerable<IMatch>>(
            context.Filter(
                new IMatch[]
                {
                    issuesFactory.Create(itemDto),
                    pullRequestsFactory.Create(itemDto),
                    milestonesFactory.Create(itemDto),
                    workflowRunsFactory.Create(itemDto)
                }
            )
        );
    }
}

internal record RepositoryMatchDto(
    Guid ConnectionId,
    string Owner,
    string RepositoryName,
    string Url
);

internal record RepositoryItemMatchDto(Guid ConnectionId, string Owner, string RepositoryName)
{
    public string GetUrl() => $"https://github.com/{Owner}/{RepositoryName}";
}
