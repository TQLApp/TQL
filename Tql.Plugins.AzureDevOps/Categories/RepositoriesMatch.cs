using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoriesMatch(
    RootItemDto dto,
    ICache<AzureData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<RepositoryMatch, RepositoryMatchDto> factory
) : CachedMatch<AzureData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.RepositoriesMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Repositories;
    public override MatchTypeId TypeId => TypeIds.Repositories;
    public override string SearchHint => Labels.RepositoriesMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(dto.Url).Projects
            from repository in project.Repositories
            select factory.Create(new RepositoryMatchDto(dto.Url, project.Name, repository.Name));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
