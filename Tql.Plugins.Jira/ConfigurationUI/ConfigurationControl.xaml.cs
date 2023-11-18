﻿using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;
using Button = System.Windows.Controls.Button;

namespace Tql.Plugins.Jira.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IUI _ui;
    private readonly JiraApi _api;
    private readonly IEncryption _encryption;
    private readonly ILogger<ConfigurationControl> _logger;
    private Guid? _id;
    private bool _dirty;

    private new ConfigurationDto DataContext => (ConfigurationDto)base.DataContext;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public Guid PageId => JiraPlugin.ConfigurationPageId;
    public string Title => Labels.ConfigurationControl_General;

    public ConfigurationControl(
        ConfigurationManager configurationManager,
        IUI ui,
        JiraApi api,
        IEncryption encryption,
        ILogger<ConfigurationControl> logger
    )
    {
        _configurationManager = configurationManager;
        _ui = ui;
        _api = api;
        _encryption = encryption;
        _logger = logger;

        InitializeComponent();

        base.DataContext = ConfigurationDto.FromConfiguration(configurationManager.Configuration);

        UpdateEnabled();
    }

    public void Initialize(IConfigurationPageContext context) { }

    private void UpdateEnabled()
    {
        _delete.IsEnabled = _connections.SelectedItem != null;
        _update.IsEnabled = CreateConnectionDto().GetIsValid();
    }

    private ConnectionDto CreateConnectionDto()
    {
        var url = _url.Text;
        if (!url.IsEmpty() && !url.EndsWith("/"))
            url += "/";

        return new ConnectionDto(_id ?? Guid.NewGuid())
        {
            Name = _name.Text,
            Url = url,
            UserName = _userName.Text,
            ProtectedPassword = _encryption.EncryptString(_password.Password)
        };
    }

    public async Task<SaveStatus> Save()
    {
        if (_update.IsEnabled)
        {
            switch (
                _ui.ShowConfirmation(
                    this,
                    "Do you want to add a new item?",
                    buttons: DialogCommonButtons.Yes
                        | DialogCommonButtons.No
                        | DialogCommonButtons.Cancel
                )
            )
            {
                case DialogResult.Yes:
                    _update.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    break;
                case DialogResult.Cancel:
                    return SaveStatus.Failure;
            }
        }

        if (!_dirty)
            return SaveStatus.Success;

        var configuration = DataContext.ToConfiguration();

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
        _userName.Text = null;
        _password.Password = null;
        _id = null;
    }

    private void _connections_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var connectionDto = (ConnectionDto)_connections.SelectedItem;

        if (connectionDto != null)
        {
            _name.Text = connectionDto.Name;
            _url.Text = connectionDto.Url;
            _userName.Text = connectionDto.UserName;
            try
            {
                _password.Password = _encryption.DecryptString(connectionDto.ProtectedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decrypt password");
                _password.Password = null;
            }
            _id = connectionDto.Id;
        }

        UpdateEnabled();
    }

    private void _name_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _url_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _userName_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _password_PasswordChanged(object sender, RoutedEventArgs e) => UpdateEnabled();

    private void _passwordDocumentation_Click(object sender, RoutedEventArgs e)
    {
        _ui.OpenUrl(
            "https://support.atlassian.com/atlassian-account/docs/manage-api-tokens-for-your-atlassian-account/"
        );
    }
}
