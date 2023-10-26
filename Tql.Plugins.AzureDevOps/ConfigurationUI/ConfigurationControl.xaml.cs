using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;

namespace Tql.Plugins.AzureDevOps.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly IConfigurationManager _configurationManager;
    private readonly ICache<AzureData> _cache;

    private new ConfigurationDto DataContext => (ConfigurationDto)base.DataContext;

    public Guid PageId => AzureDevOpsPlugin.ConfigurationPageId;
    public string Title => "General";

    public ConfigurationControl(IConfigurationManager configurationManager, ICache<AzureData> cache)
    {
        _configurationManager = configurationManager;
        _cache = cache;

        InitializeComponent();

        var configuration = Configuration.FromJson(
            configurationManager.GetConfiguration(AzureDevOpsPlugin.Id)
        );

        base.DataContext = ConfigurationDto.FromConfiguration(configuration);

        UpdateEnabled();
    }

    private void UpdateEnabled()
    {
        _delete.IsEnabled = _connections.SelectedItem != null;
        _update.IsEnabled = CreateConnectionDto().GetIsValid();
    }

    private ConnectionDto CreateConnectionDto() => new() { Name = _name.Text, Url = _url.Text };

    public Task<SaveStatus> Save()
    {
        _configurationManager.SetConfiguration(
            AzureDevOpsPlugin.Id,
            DataContext.ToConfiguration().ToJson()
        );

        _cache.Invalidate();

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
        _url.Text = null;
    }

    private void _connections_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var connectionDto = (ConnectionDto)_connections.SelectedItem;

        if (connectionDto != null)
        {
            _name.Text = connectionDto.Name;
            _url.Text = connectionDto.Url;
        }

        UpdateEnabled();
    }

    private void _name_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _url_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();
}
