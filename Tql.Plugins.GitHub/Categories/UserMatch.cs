using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class UserMatch
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch,
        ISearchableMatch,
        IHasSearchHint
{
    private readonly UserMatchDto _dto;
    private readonly GitHubApi _api;

    public string Text => _dto.Login;
    public ImageSource Icon => Images.User;
    public MatchTypeId TypeId => TypeIds.User;
    public string SearchHint => "Find repositories";

    public UserMatch(UserMatchDto dto, GitHubApi api)
    {
        _dto = dto;
        _api = api;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.Url);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.Url);

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = await _api.GetClient(_dto.ConnectionId);

        var response = await client.Search.SearchRepo(
            new SearchRepositoriesRequest(text) { User = _dto.Login }
        );

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(
            p =>
                new RepositoryMatch(
                    new RepositoryMatchDto(_dto.ConnectionId, p.FullName, p.HtmlUrl),
                    _api
                )
        );
    }
}

internal record UserMatchDto(Guid ConnectionId, string Login, string Url);
