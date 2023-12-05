using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class NewsMatch(
    RootItemDto dto,
    ICache<GitHubData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<NewMatch, NewMatchDto> factory
) : CachedMatch<GitHubData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(Labels.NewsMatch_Label, configurationManager.Configuration, dto);

    public override ImageSource Icon => Images.New;
    public override MatchTypeId TypeId => TypeIds.News;
    public override string SearchHint => Labels.NewsMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(GitHubData data)
    {
        yield return factory.Create(new NewMatchDto(null, null, null, NewMatchType.Repository));
        yield return factory.Create(
            new NewMatchDto(null, null, null, NewMatchType.ImportRepository)
        );
        yield return factory.Create(new NewMatchDto(null, null, null, NewMatchType.Codespace));
        yield return factory.Create(new NewMatchDto(null, null, null, NewMatchType.Gist));
        yield return factory.Create(new NewMatchDto(null, null, null, NewMatchType.Organization));

        foreach (var repository in data.GetConnection(dto.Id).Repositories)
        {
            yield return factory.Create(
                new NewMatchDto(dto.Id, repository.Owner, repository.Name, NewMatchType.Issue)
            );
            yield return factory.Create(
                new NewMatchDto(dto.Id, repository.Owner, repository.Name, NewMatchType.PullRequest)
            );
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
