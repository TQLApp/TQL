using Tql.Abstractions;
using Tql.Plugins.Azure.Services;
using Tql.Utilities;

namespace Tql.Plugins.Azure.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IUI _ui;

    private new ConfigurationDto DataContext => (ConfigurationDto)base.DataContext;

    public Guid PageId => AzurePlugin.ConfigurationPageId;
    public string Title => Labels.ConfigurationControl_General;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public ConfigurationControl(ConfigurationManager configurationManager, IUI ui)
    {
        _configurationManager = configurationManager;
        _ui = ui;

        InitializeComponent();

        base.DataContext = ConfigurationDto.FromConfiguration(configurationManager.Configuration);

        _connection.DataContext = null;
    }

    public void Initialize(IConfigurationPageContext context) { }

    public Task<SaveStatus> Save()
    {
        if (_connection.DataContext != null)
        {
            _ui.ShowAlert(
                this,
                Labels.ConfigurationControl_EditingConnection,
                Labels.ConfigurationControl_EditingConnectionSubtitle
            );
            return Task.FromResult(SaveStatus.Failure);
        }

        _configurationManager.UpdateConfiguration(DataContext.ToConfiguration());

        return Task.FromResult(SaveStatus.Success);
    }

    private void _add_Click(object? sender, RoutedEventArgs e)
    {
        _connections.SelectedItem = null;

        _connection.DataContext = new ConnectionDto(Guid.NewGuid());
    }

    private void _delete_Click(object? sender, RoutedEventArgs e)
    {
        DataContext.Connections.Remove((ConnectionDto)_connections.SelectedItem);

        _connection.DataContext = null;
    }

    private void _update_Click(object? sender, RoutedEventArgs e)
    {
        var connection = (ConnectionDto)_connection.DataContext;

        if (_connections.SelectedItem != null)
            DataContext.Connections[_connections.SelectedIndex] = connection;
        else
            DataContext.Connections.Add(connection);

        _connection.DataContext = null;
        _connections.SelectedItem = null;
    }

    private void _connections_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _connection.DataContext = ((ConnectionDto?)_connections.SelectedItem)?.Clone();
    }

    private void _documentation_Click(object? sender, RoutedEventArgs e)
    {
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/Azure-plugin");
    }

    private void _cancel_Click(object sender, RoutedEventArgs e)
    {
        _connection.DataContext = null;
        _connections.SelectedItem = null;
    }
}
