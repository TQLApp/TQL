using System.Windows.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceProvider _serviceProvider;

    private new ConfigurationDto DataContext => (ConfigurationDto)base.DataContext;

    public Guid PageId => JiraPlugin.ConfigurationPageId;
    public string Title => Labels.ConfigurationControl_General;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public ConfigurationControl(
        ConfigurationManager configurationManager,
        IUI ui,
        JiraApi api,
        IEncryption encryption,
        IServiceProvider serviceProvider
    )
    {
        _configurationManager = configurationManager;
        _ui = ui;
        _api = api;
        _encryption = encryption;
        _serviceProvider = serviceProvider;

        InitializeComponent();

        base.DataContext = ConfigurationDto.FromConfiguration(
            configurationManager.Configuration,
            _encryption
        );
    }

    public void Initialize(IConfigurationPageContext context) { }

    public async Task<SaveStatus> Save()
    {
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

    private void _add_Click(object? sender, RoutedEventArgs e) => Edit(null);

    private void _edit_Click(object sender, RoutedEventArgs e) =>
        Edit((ConnectionDto)_connections.SelectedItem);

    private void Edit(ConnectionDto? connection)
    {
        var editConnection = connection?.Clone() ?? new ConnectionDto(Guid.NewGuid());

        var window = _serviceProvider.GetRequiredService<ConnectionEditWindow>();

        window.Owner = Window.GetWindow(this);
        window.DataContext = editConnection;

        if (window.ShowDialog().GetValueOrDefault())
        {
            if (connection != null)
                DataContext.Connections[_connections.SelectedIndex] = editConnection;
            else
                DataContext.Connections.Add(editConnection);
        }
    }

    private void _delete_Click(object? sender, RoutedEventArgs e)
    {
        DataContext.Connections.Remove((ConnectionDto)_connections.SelectedItem);
    }

    private void _documentation_Click(object? sender, RoutedEventArgs e)
    {
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/JIRA-plugin");
    }

    private void _connections_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && _edit.IsEnabled)
            _edit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }
}
