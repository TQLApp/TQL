using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class UserMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    private readonly UserMatchDto _dto;
    private readonly GitHubApi _api;
    private readonly IMatchFactory<RepositoryMatch, RepositoryMatchDto> _factory;

    public string Text => _dto.Login;
    public ImageSource Icon => Images.User;
    public MatchTypeId TypeId => TypeIds.User;
    public string SearchHint => Labels.UserMatch_SearchHint;

    public UserMatch(
        UserMatchDto dto,
        GitHubApi api,
        IMatchFactory<RepositoryMatch, RepositoryMatchDto> factory
    )
    {
        _dto = dto;
        _api = api;
        _factory = factory;
    }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
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
            p => _factory.Create(new RepositoryMatchDto(_dto.ConnectionId, p.FullName, p.HtmlUrl))
        );
    }
}

internal record UserMatchDto(Guid ConnectionId, string Login, string Url);
