using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class MilestonesMatch(
    RepositoryItemMatchDto dto,
    GitHubApi api,
    IMatchFactory<MilestoneMatch, MilestoneMatchDto> factory
) : ISearchableMatch, ISerializableMatch
{
    public string Text =>
        MatchText.Path($"{dto.Owner}/{dto.RepositoryName}", Labels.MilestonesMatch_Label);
    public ImageSource Icon => Images.Milestone;
    public MatchTypeId TypeId => TypeIds.Milestones;
    public string SearchHint => Labels.MilestonesMatch_SearchHint;

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var task = context.GetDataCached(
            $"{GetType().FullName}|{dto.Owner}|{dto.RepositoryName}",
            _ => GetMilestones()
        );

        if (!task.IsCompleted)
            await context.DebounceDelay(cancellationToken);

        return context.Filter(await task);
    }

    private async Task<ImmutableArray<MilestoneMatch>> GetMilestones()
    {
        var client = await api.GetClient(dto.ConnectionId);

        return (
            from milestone in await client
                .Issue
                .Milestone
                .GetAllForRepository(dto.Owner, dto.RepositoryName)
            select factory.Create(
                new MilestoneMatchDto(
                    dto.ConnectionId,
                    dto.Owner,
                    dto.RepositoryName,
                    milestone.Title,
                    milestone.HtmlUrl
                )
            )
        ).ToImmutableArray();
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
