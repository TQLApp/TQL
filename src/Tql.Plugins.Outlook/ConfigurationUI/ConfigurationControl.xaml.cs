using Tql.Abstractions;
using Tql.Plugins.Outlook.Services;

namespace Tql.Plugins.Outlook.ConfigurationUI;

internal partial class ConfigurationControl : IConfigurationPage
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IUI _ui;

    public Guid PageId => OutlookPlugin.ConfigurationPageId;
    public string Title => Labels.ConfigurationControl_General;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.Scroll;

    public ConfigurationControl(ConfigurationManager configurationManager, IUI ui)
    {
        _configurationManager = configurationManager;
        _ui = ui;

        InitializeComponent();

        var configuration = configurationManager.Configuration;

        AddNameFormat(Labels.NameFormat_None, NameFormat.None);
        AddNameFormat(Labels.NameFormat_LastNameCommaFirstName, NameFormat.LastNameCommaFirstName);

        void AddNameFormat(string label, NameFormat nameFormat)
        {
            _nameFormat.Items.Add(
                new ComboBoxItem
                {
                    Content = label,
                    IsSelected = configuration.NameFormat == nameFormat,
                    Tag = nameFormat
                }
            );
        }
    }

    public void Initialize(IConfigurationPageContext context) { }

    public Task<SaveStatus> Save()
    {
        var nameFormat = (NameFormat)((ComboBoxItem)_nameFormat.SelectedItem).Tag;

        _configurationManager.UpdateConfiguration(
            _configurationManager.Configuration with
            {
                NameFormat = nameFormat
            }
        );

        return Task.FromResult(SaveStatus.Success);
    }

    private void _documentation_Click(object? sender, RoutedEventArgs e)
    {
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/Outlook-plugin");
    }
}
