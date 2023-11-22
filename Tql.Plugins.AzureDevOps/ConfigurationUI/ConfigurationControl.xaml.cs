using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;
using Button = System.Windows.Controls.Button;

namespace Tql.Plugins.AzureDevOps.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IUI _ui;
    private readonly IEncryption _encryption;
    private readonly ILogger<ConfigurationControl> _logger;
    private Guid? _id;

    private new ConfigurationDto DataContext => (ConfigurationDto)base.DataContext;

    public Guid PageId => AzureDevOpsPlugin.ConfigurationPageId;
    public string Title => Labels.ConfigurationControl_General;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public ConfigurationControl(
        ConfigurationManager configurationManager,
        IUI ui,
        IEncryption encryption,
        ILogger<ConfigurationControl> logger
    )
    {
        _configurationManager = configurationManager;
        _ui = ui;
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
        return new ConnectionDto(_id ?? Guid.NewGuid())
        {
            Name = _name.Text,
            Url = _url.Text,
            ProtectedPATToken = _encryption.EncryptString(_patToken.Password)
        };
    }

    public Task<SaveStatus> Save()
    {
        if (_update.IsEnabled)
        {
            switch (
                _ui.ShowConfirmation(
                    this,
                    Labels.Confirm_DoYouWantToAddNewItem,
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
                    return Task.FromResult(SaveStatus.Failure);
            }
        }

        _configurationManager.UpdateConfiguration(DataContext.ToConfiguration());

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

            try
            {
                _patToken.Password = _encryption.DecryptString(connectionDto.ProtectedPATToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decrypt password");
                _patToken.Password = null;
            }

            _id = connectionDto.Id;
        }

        UpdateEnabled();
    }

    private void _name_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _url_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _patToken_PasswordChanged(object sender, RoutedEventArgs e) => UpdateEnabled();

    private void _documentation_Click(object sender, RoutedEventArgs e)
    {
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/Azure-DevOps-plugin");
    }
}
