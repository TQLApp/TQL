using JetBrains.Annotations;
using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;

namespace Tql.Plugins.MicrosoftTeams.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IUI _ui;

    public Guid PageId => MicrosoftTeamsPlugin.ConfigurationPageId;
    public string Title => Labels.ConfigurationControl_General;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public ConfigurationControl(
        ConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager,
        IUI ui
    )
    {
        _configurationManager = configurationManager;
        _ui = ui;

        InitializeComponent();

        var configuration = configurationManager.Configuration;

        _allDirectories.IsChecked = configuration.Mode == ConfigurationMode.All;
        _selectedDirectories.IsChecked = configuration.Mode == ConfigurationMode.Selected;

        _directories.ItemsSource = peopleDirectoryManager
            .Directories
            .OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(
                p => new DirectoryItem(p) { IsSelected = configuration.DirectoryIds.Contains(p.Id) }
            )
            .ToList();

        UpdateEnabled();
    }

    public void Initialize(IConfigurationPageContext context) { }

    private void UpdateEnabled()
    {
        _directories.IsEnabled = _selectedDirectories.IsChecked.GetValueOrDefault();
    }

    public Task<SaveStatus> Save()
    {
        var directoryIds = _directories
            .ItemsSource
            .Cast<DirectoryItem>()
            .Where(p => p.IsSelected)
            .Select(p => p.Directory.Id)
            .ToImmutableArray();

        var configuration = new Configuration(
            _allDirectories.IsChecked.GetValueOrDefault()
                ? ConfigurationMode.All
                : ConfigurationMode.Selected,
            directoryIds
        );

        _configurationManager.UpdateConfiguration(configuration);

        return Task.FromResult(SaveStatus.Success);
    }

    private void _selectedDirectories_Checked(object sender, RoutedEventArgs e) => UpdateEnabled();

    private void _selectedDirectories_Unchecked(object sender, RoutedEventArgs e) =>
        UpdateEnabled();

    private void _documentation_Click(object sender, RoutedEventArgs e)
    {
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/Microsoft-Teams-plugin");
    }

    private record DirectoryItem(IPeopleDirectory Directory)
    {
        [UsedImplicitly]
        public bool IsSelected { get; set; }
    }
}
