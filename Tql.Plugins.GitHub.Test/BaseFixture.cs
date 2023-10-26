using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.PluginTestSupport;

namespace Tql.Plugins.GitHub.Test;

internal class BaseFixture : PluginTestFixture
{
    public override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        const string environmentVariableName = "TQL_GitHubPatToken";

        var patToken = Environment.GetEnvironmentVariable(environmentVariableName);

        if (string.IsNullOrEmpty(patToken))
        {
            throw new InvalidOperationException(
                $"GitHub PAT token must be available in an environment variable named '{environmentVariableName}'"
            );
        }

        Services
            .GetRequiredService<IConfigurationManager>()
            .SetConfiguration(
                GitHubPlugin.Id,
                JsonSerializer.Serialize(
                    new Configuration(
                        ImmutableArray.Create(new Connection(Guid.NewGuid(), "test", patToken))
                    )
                )
            );
    }

    protected override IEnumerable<ITqlPlugin> GetPlugins()
    {
        yield return new GitHubPlugin();
    }
}
