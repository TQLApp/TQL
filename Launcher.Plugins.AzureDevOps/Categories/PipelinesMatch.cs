using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class PipelinesMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly string _url;

    public override string Text { get; }
    public override ImageSource Icon => Images.Pipelines;
    public override MatchTypeId TypeId => TypeIds.Pipelines;

    public PipelinesMatch(string text, string url, ICache<AzureData> cache)
        : base(cache)
    {
        _url = url;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(_url).Projects
            from buildDefinition in project.BuildDefinitions
            select new PipelineMatch(
                new PipelineMatchDto(
                    _url,
                    project.Name,
                    buildDefinition.Id,
                    buildDefinition.Path,
                    buildDefinition.Name
                )
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
