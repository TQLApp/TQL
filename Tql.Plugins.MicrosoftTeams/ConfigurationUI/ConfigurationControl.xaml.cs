using Tql.Abstractions;

namespace Tql.Plugins.MicrosoftTeams.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly IConfigurationManager _configurationManager;

    public Guid PageId => MicrosoftTeamsPlugin.ConfigurationPageId;
    public string Title => "General";
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public ConfigurationControl(
        IConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager
    )
    {
        _configurationManager = configurationManager;

        InitializeComponent();

        var configuration = Configuration.FromJson(
            configurationManager.GetConfiguration(MicrosoftTeamsPlugin.Id)
        );

        _allDirectories.IsChecked = configuration.Mode == ConfigurationMode.All;

        foreach (var directory in peopleDirectoryManager.Directories)
        {
            _directories.Items.Add(
                new ListBoxItem
                {
                    Content = directory.Name,
                    Tag = directory,
                    IsSelected = configuration.DirectoryIds.Contains(directory.Id)
                }
            );
        }

        UpdateEnabled();
    }

    private void UpdateEnabled()
    {
        _directories.IsEnabled = _selectedDirectories.IsChecked.GetValueOrDefault();
    }

    public Task<SaveStatus> Save()
    {
        var directoryIds = _directories.Items
            .Cast<ListBoxItem>()
            .Select(p => ((IPeopleDirectory)p.Tag).Id)
            .ToImmutableArray();

        var configuration = new Configuration(
            _allDirectories.IsChecked.GetValueOrDefault()
                ? ConfigurationMode.All
                : ConfigurationMode.Selected,
            directoryIds
        );

        _configurationManager.SetConfiguration(MicrosoftTeamsPlugin.Id, configuration.ToJson());

        return Task.FromResult(SaveStatus.Success);
    }

    private void _selectedDirectories_Checked(object sender, RoutedEventArgs e) => UpdateEnabled();

    private void _selectedDirectories_Unchecked(object sender, RoutedEventArgs e) =>
        UpdateEnabled();
}
