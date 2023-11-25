using System.Windows.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IUI _ui;
    private readonly IServiceProvider _serviceProvider;

    private new ConfigurationDto DataContext => (ConfigurationDto)base.DataContext;

    public Guid PageId => GitHubPlugin.ConfigurationPageId;
    public string Title => Labels.ConfigurationControl_General;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public ConfigurationControl(
        ConfigurationManager configurationManager,
        IUI ui,
        IServiceProvider serviceProvider
    )
    {
        _configurationManager = configurationManager;
        _ui = ui;
        _serviceProvider = serviceProvider;

        InitializeComponent();

        base.DataContext = ConfigurationDto.FromConfiguration(configurationManager.Configuration);
    }

    public void Initialize(IConfigurationPageContext context) { }

    public Task<SaveStatus> Save()
    {
        _configurationManager.UpdateConfiguration(DataContext.ToConfiguration());

        return Task.FromResult(SaveStatus.Success);
    }

    private void _add_Click(object? sender, RoutedEventArgs e) => Edit(null);

    private void _edit_Click(object sender, RoutedEventArgs e) =>
        Edit((ConnectionDto)_connections.SelectedItem);

    private void Edit(ConnectionDto? connection)
    {
        var editConnection = connection?.Clone() ?? new ConnectionDto(Guid.NewGuid(), null, null);

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
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/GitHub-plugin");
    }

    private void _connections_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && _edit.IsEnabled)
            _edit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }
}
