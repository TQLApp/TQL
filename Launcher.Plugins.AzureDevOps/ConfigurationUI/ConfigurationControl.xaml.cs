using Launcher.Abstractions;

namespace Launcher.Plugins.AzureDevOps.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationUI
{
    public bool IsValid => ((ConfigurationDto)DataContext).IsValid;

    public ConfigurationControl(IConfigurationManager configurationManager)
    {
        InitializeComponent();

        var configuration = Configuration.FromJson(
            configurationManager.GetConfiguration(AzureDevOpsPlugin.Id)
        );

        DataContext = ConfigurationDto.FromConfiguration(configuration);
    }

    public string GetConfiguration()
    {
        return ((ConfigurationDto)DataContext).ToConfiguration().ToJson();
    }
}
