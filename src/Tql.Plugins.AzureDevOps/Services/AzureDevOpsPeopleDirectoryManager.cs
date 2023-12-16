using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Services;

internal class AzureDevOpsPeopleDirectoryManager
{
    private readonly ConfigurationManager _configurationManager;
    private readonly AzureDevOpsApi _api;
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;

    public AzureDevOpsPeopleDirectoryManager(
        ConfigurationManager configurationManager,
        AzureDevOpsApi api,
        IPeopleDirectoryManager peopleDirectoryManager
    )
    {
        _configurationManager = configurationManager;
        _api = api;
        _peopleDirectoryManager = peopleDirectoryManager;

        configurationManager.Changed += (_, _) => Reload();

        Reload();
    }

    private void Reload()
    {
        foreach (
            var directory in _peopleDirectoryManager.Directories.OfType<AzureDevOpsPeopleDirectory>()
        )
        {
            _peopleDirectoryManager.Remove(directory);
        }

        foreach (var connection in _configurationManager.Configuration.Connections)
        {
            _peopleDirectoryManager.Add(new AzureDevOpsPeopleDirectory(connection, _api));
        }
    }
}
