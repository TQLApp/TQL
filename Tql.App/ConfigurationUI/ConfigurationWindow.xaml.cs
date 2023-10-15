using Tql.Abstractions;
using Tql.App.Services;

namespace Tql.App.ConfigurationUI;

internal partial class ConfigurationWindow
{
    public ConfigurationWindow(
        IServiceProvider serviceProvider,
        IConfigurationManager configurationManager
    )
    {
        InitializeComponent();

        var factories = ((ConfigurationManager)configurationManager).ConfigurationUIFactories;

        foreach (
            var factory in factories.OrderBy(p => p.Title, StringComparer.CurrentCultureIgnoreCase)
        )
        {
            var ui = (UIElement)factory.CreateControl(serviceProvider);

            _pages.Items.Add(new TreeViewItem { Header = factory.Title, Tag = ui });
        }

        if (_pages.Items.Count > 0)
            ((TreeViewItem)_pages.Items[0]!).IsSelected = true;
    }

    private void _pages_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _container.Child = (UIElement?)((TreeViewItem?)e.NewValue)?.Tag;
    }

    private void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (TreeViewItem page in _pages.Items)
        {
            var status = ((IConfigurationUI)page.Tag).Save();
            if (status == SaveStatus.Failure)
                return;
        }

        DialogResult = true;
    }
}
