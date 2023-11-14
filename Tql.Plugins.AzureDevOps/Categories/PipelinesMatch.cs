using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class PipelinesMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<PipelineMatch, PipelineMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.PipelinesType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Pipelines;
    public override MatchTypeId TypeId => TypeIds.Pipelines;

    public PipelinesMatch(
        RootItemDto dto,
        ICache<AzureData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<PipelineMatch, PipelineMatchDto> factory
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
            from buildDefinition in project.BuildDefinitions
            select _factory.Create(
                new PipelineMatchDto(
                    _dto.Url,
                    project.Name,
                    buildDefinition.Id,
                    buildDefinition.Path,
                    buildDefinition.Name
                )
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
