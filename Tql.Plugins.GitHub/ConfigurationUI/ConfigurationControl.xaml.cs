using Tql.Abstractions;

namespace Tql.Plugins.GitHub.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly IConfigurationManager _configurationManager;
    private Guid? _id;

    private new ConfigurationDto DataContext => (ConfigurationDto)base.DataContext;

    public Guid PageId => GitHubPlugin.ConfigurationPageId;
    public string Title => "General";

    public ConfigurationControl(IConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;

        InitializeComponent();

        var configuration = Configuration.FromJson(
            configurationManager.GetConfiguration(GitHubPlugin.Id)
        );

        base.DataContext = ConfigurationDto.FromConfiguration(configuration);

        UpdateEnabled();
    }

    private void UpdateEnabled()
    {
        _delete.IsEnabled = _connections.SelectedItem != null;
        _update.IsEnabled = CreateConnectionDto().GetIsValid();
    }

    private ConnectionDto CreateConnectionDto() => new(_id ?? Guid.NewGuid()) { Name = _name.Text };

    public Task<SaveStatus> Save()
    {
        _configurationManager.SetConfiguration(
            GitHubPlugin.Id,
            DataContext.ToConfiguration().ToJson()
        );

        return Task.FromResult(SaveStatus.Success);
    }

    private void _add_Click(object sender, RoutedEventArgs e)
    {
        _connections.SelectedItem = null;

        ClearEdit();
    }

    private void _delete_Click(object sender, RoutedEventArgs e)
    {
        DataContext.Connections.Remove((ConnectionDto)_connections.SelectedItem);

        ClearEdit();
    }

    private void _update_Click(object sender, RoutedEventArgs e)
    {
        if (_connections.SelectedItem != null)
            DataContext.Connections[_connections.SelectedIndex] = CreateConnectionDto();
        else
            DataContext.Connections.Add(CreateConnectionDto());

        ClearEdit();
    }

    private void ClearEdit()
    {
        _name.Text = null;
        _id = null;
    }

    private void _connections_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var connectionDto = (ConnectionDto)_connections.SelectedItem;

        if (connectionDto != null)
        {
            _name.Text = connectionDto.Name;
            _id = connectionDto.Id;
        }

        UpdateEnabled();
    }

    private void _name_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();
}
