using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class NewPullRequestsMatch(
    NewMatchDto dto,
    GitHubApi api,
    IMatchFactory<NewPullRequestMatch, NewPullRequestDto> factory
) : ISerializableMatch, ISearchableMatch
{
    public string Text =>
        MatchText.Path($"{dto.Owner}/{dto.Repository}", Labels.NewMatch_NewPullRequest);
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
        var task = context.GetDataCached($"{GetType()}|{dto.Id}", _ => GetMatches());

        if (!task.IsCompleted)
            await context.DebounceDelay(cancellationToken);

        return context.Filter(await task);
    }

    private async Task<ImmutableArray<NewPullRequestMatch>> GetMatches()
    {
        var graphQlConnection = await api.GetConnection(dto.Id!.Value);

        var query = new Query()
            .Repository(dto.Repository, dto.Owner)
            .Refs("refs/heads/", first: 100)
            .Edges.Select(
                refEdge =>
                    new
                    {
                        Refs = refEdge
                            .Node.Select(
                                @ref =>
                                    new
                                    {
                                        @ref.Name,
                                        AssociatedPullRequests = @ref.AssociatedPullRequests(
                                            1,
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            null,
                                            new[] { PullRequestState.Open }
                                        )
                                            .Edges.Select(
                                                pullRequest =>
                                                    new
                                                    {
                                                        Id = pullRequest
                                                            .Select(p4 => p4.Node.Id)
                                                            .SingleOrDefault()
                                                    }
                                            )
                                            .ToList()
                                    }
                            )
                            .SingleOrDefault()
                    }
            )
            .Compile();

        var items = await graphQlConnection.Run(query);

        return items
            .Where(p => p.Refs.AssociatedPullRequests.Count == 0)
            .Select(
                p =>
                    factory.Create(
                        new NewPullRequestDto(
                            dto.Id!.Value,
                            dto.Owner!,
                            dto.Repository!,
                            p.Refs.Name
                        )
                    )
            )
            .ToImmutableArray();
    }
}
