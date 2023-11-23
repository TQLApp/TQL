using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IUI _ui;
    private readonly JiraApi _api;
    private readonly IEncryption _encryption;

    private new ConfigurationDto DataContext => (ConfigurationDto)base.DataContext;

    public Guid PageId => JiraPlugin.ConfigurationPageId;
    public string Title => Labels.ConfigurationControl_General;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public ConfigurationControl(
        ConfigurationManager configurationManager,
        IUI ui,
        JiraApi api,
        IEncryption encryption
    )
    {
        _configurationManager = configurationManager;
        _ui = ui;
        _api = api;
        _encryption = encryption;

        InitializeComponent();

        base.DataContext = ConfigurationDto.FromConfiguration(
            configurationManager.Configuration,
            _encryption
        );

        _connection.DataContext = null;
    }

    public void Initialize(IConfigurationPageContext context) { }

    public async Task<SaveStatus> Save()
    {
        if (_connection.DataContext != null)
        {
            _ui.ShowAlert(
                this,
                Labels.ConfigurationControl_EditingConnection,
                Labels.ConfigurationControl_EditingConnectionSubtitle
            );
            return SaveStatus.Failure;
        }

        var configuration = DataContext.ToConfiguration(_encryption);

        foreach (var connection in configuration.Connections)
        {
            try
            {
                await _api.GetClient(connection).GetDashboards(1);
            }
            catch (Exception ex)
            {
                _ui.ShowException(this, $"Failed to connect to {connection.Name}", ex);
                return SaveStatus.Failure;
            }
        }

        _configurationManager.UpdateConfiguration(configuration);

        return SaveStatus.Success;
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
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/JIRA-plugin");
    }

    private void _cancel_Click(object sender, RoutedEventArgs e)
    {
        _connection.DataContext = null;
        _connections.SelectedItem = null;
    }
}
