using Tql.Abstractions;
using Tql.PluginTestSupport;

namespace Tql.Plugins.Demo.Test;

internal class BaseFixture : PluginTestFixture
{
    protected override IEnumerable<ITqlPlugin> GetPlugins()
    {
        yield return new DemoPlugin();
    }
}
