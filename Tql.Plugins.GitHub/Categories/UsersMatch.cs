using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class UsersMatch : ISearchableMatch, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly GitHubApi _api;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<UserMatch, UserMatchDto> _factory;

    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.UsersMatch_Label,
            _configurationManager.Configuration,
            _dto
        );

    public ImageSource Icon => Images.User;
    public MatchTypeId TypeId => TypeIds.Users;
    public string SearchHint => Labels.UsersMatch_SearchHint;

    public UsersMatch(
        RootItemDto dto,
        GitHubApi api,
        ConfigurationManager configurationManager,
        IMatchFactory<UserMatch, UserMatchDto> factory
    )
    {
        _dto = dto;
        _api = api;
        _configurationManager = configurationManager;
        _factory = factory;
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

        return response
            .Items
            .Select(p => _factory.Create(new UserMatchDto(_dto.Id, p.Login, p.HtmlUrl)));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
