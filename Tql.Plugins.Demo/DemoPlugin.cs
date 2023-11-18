using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Demo.Categories;
using Tql.Plugins.Demo.Services;
using Tql.Utilities;

namespace Tql.Plugins.Demo;

[TqlPlugin]
public class DemoPlugin : ITqlPlugin
{
    public static readonly Guid Id = Guid.Parse("7a44cf85-6b57-4ce1-b057-bdbb9aea50fe");

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private IMatchTypeManager? _matchTypeManager;
    private IMatchFactory<DemoesMatch, DemoesMatchDto>? _factory;

    Guid ITqlPlugin.Id => Id;
    public string Title => Labels.DemoPlugin_Label;

    public DemoPlugin()
    {
        _matchTypeManagerBuilder = MatchTypeManagerBuilder.ForAssembly(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        _matchTypeManagerBuilder.ConfigureServices(services);
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        serviceProvider
            .GetRequiredService<IPeopleDirectoryManager>()
            .Add(new DemoPeopleDirectory());

        _matchTypeManager = _matchTypeManagerBuilder.Build(serviceProvider);
        _factory = serviceProvider.GetRequiredService<IMatchFactory<DemoesMatch, DemoesMatchDto>>();
    }

    public IEnumerable<IMatch> GetMatches()
    {
        yield return _factory!.Create(new DemoesMatchDto());
    }

    public IMatch? DeserializeMatch(Guid typeId, string value)
    {
        return _matchTypeManager?.Deserialize(typeId, value);
    }

    public IEnumerable<IConfigurationPage> GetConfigurationPages() =>
        Array.Empty<IConfigurationPage>();
}
