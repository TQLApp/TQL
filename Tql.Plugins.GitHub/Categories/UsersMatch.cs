using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

internal class UsersMatch : ISearchableMatch, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly GitHubApi _api;

    public string Text { get; }
    public ImageSource Icon => Images.User;
    public MatchTypeId TypeId => TypeIds.Users;

    public UsersMatch(string text, RootItemDto dto, GitHubApi api)
    {
        _dto = dto;
        _api = api;

        Text = text;
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

        var client = await _api.GetClient(_dto.Id);

        var response = await client.Search.SearchUsers(new SearchUsersRequest(text));

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(
            p => new UserMatch(new UserMatchDto(_dto.Id, p.Login, p.HtmlUrl), _api)
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
