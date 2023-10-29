using System.Windows.Forms;
using Tql.Abstractions;
using Tql.App.Services;
using Tql.App.Services.Packages;
using Button = System.Windows.Controls.Button;

namespace Tql.App.ConfigurationUI;

internal partial class PackageSourcesConfigurationControl : IConfigurationPage
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IUI _ui;
    private bool _dirty;

    private new PackageManagerConfigurationDto DataContext =>
        (PackageManagerConfigurationDto)base.DataContext;

    public Guid PageId => Constants.PackageManagerPageId;
    public string Title => "Package Sources";
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public PackageSourcesConfigurationControl(IConfigurationManager configurationManager, IUI ui)
    {
        _configurationManager = configurationManager;
        _ui = ui;

        InitializeComponent();

        var configuration = PackageManagerConfiguration.FromJson(
            configurationManager.GetConfiguration(Constants.PackageManagerConfigurationId)
        );

        base.DataContext = PackageManagerConfigurationDto.FromConfiguration(configuration);

        UpdateEnabled();
    }

    public void Initialize(IConfigurationPageContext context) { }

    private void UpdateEnabled()
    {
        _delete.IsEnabled = _sources.SelectedItem != null;
        _update.IsEnabled = CreateSourceDto().GetIsValid();
    }

    private PackageManagerSourceDto CreateSourceDto()
    {
        return new PackageManagerSourceDto
        {
            Url = _url.Text,
            UserName = _userName.Text,
            Password = _password.Password
        };
    }

    public Task<SaveStatus> Save()
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
                    return Task.FromResult(SaveStatus.Failure);
            }
        }

        if (!_dirty)
            return Task.FromResult(SaveStatus.Success);

        _configurationManager.SetConfiguration(
            Constants.PackageManagerConfigurationId,
            DataContext.ToConfiguration().ToJson()
        );

        return Task.FromResult(SaveStatus.Success);
    }

    private void _add_Click(object sender, RoutedEventArgs e)
    {
        _sources.SelectedItem = null;

        ClearEdit();
    }

    private void _delete_Click(object sender, RoutedEventArgs e)
    {
        DataContext.Sources.Remove((PackageManagerSourceDto)_sources.SelectedItem);

        _dirty = true;

        ClearEdit();
    }

    private void _update_Click(object sender, RoutedEventArgs e)
    {
        if (_sources.SelectedItem != null)
            DataContext.Sources[_sources.SelectedIndex] = CreateSourceDto();
        else
            DataContext.Sources.Add(CreateSourceDto());

        _dirty = true;

        ClearEdit();
    }

    private void ClearEdit()
    {
        _url.Text = null;
        _userName.Text = null;
        _password.Password = null;
    }

    private void _sources_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var connectionDto = (PackageManagerSourceDto)_sources.SelectedItem;

        if (connectionDto != null)
        {
            _url.Text = connectionDto.Url;
            _userName.Text = connectionDto.UserName;
            _password.Password = connectionDto.Password;
        }

        UpdateEnabled();
    }

    private void _url_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _userName_TextChanged(object sender, TextChangedEventArgs e) => UpdateEnabled();

    private void _password_PasswordChanged(object sender, RoutedEventArgs e) => UpdateEnabled();
}
