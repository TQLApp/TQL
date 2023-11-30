using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class PipelinesMatch(
    RootItemDto dto,
    ICache<AzureData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<PipelineMatch, PipelineMatchDto> factory
) : CachedMatch<AzureData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.PipelinesMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Pipelines;
    public override MatchTypeId TypeId => TypeIds.Pipelines;
    public override string SearchHint => Labels.PipelinesMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(dto.Url).Projects
            from buildDefinition in project.BuildDefinitions
            select factory.Create(
                new PipelineMatchDto(
                    dto.Url,
                    project.Name,
                    buildDefinition.Id,
                    buildDefinition.Path,
                    buildDefinition.Name
                )
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
