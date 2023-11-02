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
    private MatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;
    public string Title => Labels.DemoPlugin_Label;

    public DemoPlugin()
    {
        _matchTypeManagerBuilder = new MatchTypeManagerBuilder(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        _matchTypeManagerBuilder.ConfigureServices(services);
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        serviceProvider
            .GetRequiredService<IPeopleDirectoryManager>()
            .Add(new DemoPeopleDirectory());

        _matchTypeManager = _matchTypeManagerBuilder.Build(serviceProvider);
    }

    public IEnumerable<IMatch> GetMatches()
    {
        yield return new DemoesMatch();
    }

    public IMatch? DeserializeMatch(Guid typeId, string json)
    {
        return _matchTypeManager?.Deserialize(typeId, json);
    }

    public IEnumerable<IConfigurationPage> GetConfigurationPages() =>
        Array.Empty<IConfigurationPage>();
}
