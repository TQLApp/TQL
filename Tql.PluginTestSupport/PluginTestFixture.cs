using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tql.Abstractions;
using Tql.App.Search;
using Tql.PluginTestSupport.Services;

namespace Tql.PluginTestSupport;

public abstract class PluginTestFixture
{
    static PluginTestFixture()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
        {
            // When an assembly fails to resolve, retry splitting out
            // all details except for the assembly name. I couldn't
            // get binding redirects to work. This is a work around.

            var assemblyName = e.Name.Split(',').First().Trim();

            return Assembly.Load(assemblyName);
        };
    }

    private IHost? _host;
    private ImmutableArray<ITqlPlugin> _plugins;

    public IServiceProvider Services
    {
        get
        {
            if (_host == null)
                throw new InvalidOperationException("Hosts has not been initialized");

            return _host.Services;
        }
    }

    [OneTimeSetUp]
    public virtual void OneTimeSetUp()
    {
        _plugins = GetPlugins().ToImmutableArray();

        var builder = Host.CreateApplicationBuilder();

        ConfigureServices(builder.Services);

        foreach (var plugin in _plugins)
        {
            plugin.ConfigureServices(builder.Services);
        }

        _host = builder.Build();

        foreach (var plugin in _plugins)
        {
            plugin.Initialize(_host.Services);
        }
    }

    protected virtual void ConfigureServices(IServiceCollection builder)
    {
        builder.AddSingleton<IStore, TestStore>();
        builder.AddSingleton<IConfigurationManager, TestConfigurationManager>();
        builder.AddSingleton<IUI, TestUI>();
        builder.AddSingleton<IClipboard, TestClipboard>();
        builder.AddSingleton<HttpClient>();
        builder.AddSingleton<IPeopleDirectoryManager, TestPeopleDirectoryManager>();

        builder.Add(ServiceDescriptor.Singleton(typeof(ICache<>), typeof(TestCache<>)));
    }

    [OneTimeTearDown]
    public virtual void OneTimeTearDown()
    {
        _host?.Dispose();
        _host = null;
    }

    protected abstract IEnumerable<ITqlPlugin> GetPlugins();

    public async Task<ImmutableArray<IMatch>> Search(params string[] searches)
    {
        if (searches.Length == 0)
            return ImmutableArray<IMatch>.Empty;

        var contextMap = new Dictionary<string, object>();

        List<SearchResult> items;
        var lastSearch = searches[0];

        using (var context = (SearchContext)CreateSearchContext(lastSearch, contextMap))
        {
            // Get the root items from all plugins.

            items = context
                .Filter(_plugins.SelectMany(p => p.GetMatches()))
                .Select(context.GetSearchResult)
                .Where(p => !p.IsFuzzyMatch)
                .ToList();
        }

        foreach (var search in searches.Skip(1))
        {
            Assert.AreNotEqual(0, items.Count, $"'{lastSearch}' did not match any items");

            if (items.Count > 1)
            {
                var exactMatches = items
                    .Where(
                        p => string.Equals(p.Text, lastSearch, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();

                Assert.AreEqual(
                    1,
                    exactMatches.Count,
                    $"'{lastSearch}' Search matched multiple items without an exact match"
                );

                items = exactMatches;
            }

            var searchable = items[0].Match as ISearchableMatch;

            Assert.NotNull(searchable, "Category match is not searchable");

            lastSearch = search;

            using (var context = (SearchContext)CreateSearchContext(lastSearch, contextMap))
            {
                var matches = await searchable!.Search(context, lastSearch, default);

                items = matches.Select(context.GetSearchResult).ToList();
            }
        }

        return items.Select(p => p.Match).ToImmutableArray();
    }

    public ISearchContext CreateSearchContext(
        string search,
        Dictionary<string, object>? contextMap = null
    )
    {
        return new SearchContext(Services, search, null, contextMap ?? new());
    }
}
