using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class UsersMatch(
    RootItemDto dto,
    GitHubApi api,
    ConfigurationManager configurationManager,
    IMatchFactory<UserMatch, UserMatchDto> factory
) : ISearchableMatch, ISerializableMatch
{
    public string Text =>
        MatchUtils.GetMatchLabel(Labels.UsersMatch_Label, configurationManager.Configuration, dto);

    public ImageSource Icon => Images.User;
    public MatchTypeId TypeId => TypeIds.Users;
    public string SearchHint => Labels.UsersMatch_SearchHint;

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = await api.GetClient(dto.Id);

        var response = await client.Search.SearchUsers(new SearchUsersRequest(text));

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(p =>
            factory.Create(new UserMatchDto(dto.Id, p.Login, p.HtmlUrl))
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
