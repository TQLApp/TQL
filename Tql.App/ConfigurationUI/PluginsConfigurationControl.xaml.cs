using Tql.Abstractions;

namespace Tql.App.ConfigurationUI;

internal partial class PluginsConfigurationControl : IConfigurationPage
{
    public Guid PageId => Guid.Parse("96260cfa-5814-4ed4-ac69-fcc63e4f4571");
    public string Title => "Plugins";

    public PluginsConfigurationControl()
    {
        InitializeComponent();

        SetSelectedTab(Tab.Browse);
    }

    private Tab GetSelectedTab()
    {
        if (_browseTab.IsChecked.GetValueOrDefault())
            return Tab.Browse;
        return Tab.Installed;
    }

    private void SetSelectedTab(Tab tab)
    {
        _browseTab.IsChecked = tab == Tab.Browse;
        _installedTab.IsChecked = tab == Tab.Installed;
    }

    public Task<SaveStatus> Save()
    {
        throw new NotImplementedException();
    }

    private void _browseTab_Checked(object sender, RoutedEventArgs e) => SetSelectedTab(Tab.Browse);

    private void _installedTab_Checked(object sender, RoutedEventArgs e) =>
        SetSelectedTab(Tab.Installed);

    private enum Tab
    {
        Browse,
        Installed
    }
}
