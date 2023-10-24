using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationUI
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IUI _ui;
    private Guid? _id;
    private bool _dirty;

    private new ConfigurationDto DataContext => (ConfigurationDto)base.DataContext;

    public ConfigurationControl(IConfigurationManager configurationManager, IUI ui)
    {
        _configurationManager = configurationManager;
        _ui = ui;

        InitializeComponent();

        var configuration = Configuration.FromJson(
            configurationManager.GetConfiguration(JiraPlugin.Id)
        );

        base.DataContext = ConfigurationDto.FromConfiguration(configuration);

        UpdateEnabled();
    }

    private void UpdateEnabled()
    {
        _delete.IsEnabled = _connections.SelectedItem != null;
        _update.IsEnabled = CreateConnectionDto().GetIsValid();
    }

    private ConnectionDto CreateConnectionDto() =>
        new(_id ?? Guid.NewGuid())
        {
            Name = _name.Text,
            Url = _url.Text,
            PatToken = _patToken.Password
        };

    public async Task<SaveStatus> Save()
    {
        if (!_dirty)
            return SaveStatus.Success;

        var configuration = DataContext.ToConfiguration();

        foreach (var connection in configuration.Connections)
        {
            try
            {
                var client = JiraApi.CreateClient(connection);

                _ = await client.Users.GetMyselfAsync();
            }
            catch (Exception ex)
            {
                _ui.ShowError(this, $"Failed to connect to {connection.Name}", ex);
                return SaveStatus.Failure;
            }
        }

        _configurationManager.SetConfiguration(JiraPlugin.Id, configuration.ToJson());

        return SaveStatus.Success;
    }

    private void _add_Click(object sender, RoutedEventArgs e)
    {
        _connections.SelectedItem = null;

        ClearEdit();
    }

    private void _delete_Click(object sender, RoutedEventArgs e)
    {
        DataContext.Connections.Remove((ConnectionDto)_connections.SelectedItem);

        _dirty = true;

        ClearEdit();
    }

    private void _update_Click(object sender, RoutedEventArgs e)
    {
        if (_connections.SelectedItem != null)
            DataContext.Connections[_connections.SelectedIndex] = CreateConnectionDto();
        else
            DataContext.Connections.Add(CreateConnectionDto());

        _dirty = true;

        ClearEdit();
    }

    private void ClearEdit()
    {
        _name.Text = null;
        _url.Text = null;
        _patToken.Password = null;
        _id = null;
    }

    private void _connections_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var connectionDto = (ConnectionDto)_connections.SelectedItem;

        if (connectionDto != null)
        {
            _name.Text = connectionDto.Name;
            _url.Text = connectionDto.Url;
            _patToken.Password = connectionDto.PatToken;
            _id = connectionDto.Id;
        }

        UpdateEnabled();
    }

    private void _name_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _url_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _patToken_PasswordChanged(object sender, RoutedEventArgs e) => UpdateEnabled();

    private void _patTokenDocumentation_Click(object sender, RoutedEventArgs e)
    {
        _ui.OpenUrl(
            "https://confluence.atlassian.com/enterprise/using-personal-access-tokens-1026032365.html#UsingPersonalAccessTokens-CreatingPATsintheapplication"
        );
    }
}
