using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoriesMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<RepositoryMatch, RepositoryMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.RepositoriesMatch_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Repositories;
    public override MatchTypeId TypeId => TypeIds.Repositories;
    public override string SearchHint => Labels.RepositoriesMatch_SearchHint;

    public RepositoriesMatch(
        RootItemDto dto,
        ICache<AzureData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<RepositoryMatch, RepositoryMatchDto> factory
    )
        : base(cache)
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(_dto.Url).Projects
            from repository in project.Repositories
            select _factory.Create(new RepositoryMatchDto(_dto.Url, project.Name, repository.Name));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
