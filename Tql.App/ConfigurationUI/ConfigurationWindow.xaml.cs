using Tql.Abstractions;
using Tql.App.Services;

namespace Tql.App.ConfigurationUI;

internal partial class ConfigurationWindow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationManager _configurationManager;
    public Guid? StartupPage { get; set; }

    public ConfigurationWindow(
        IServiceProvider serviceProvider,
        IConfigurationManager configurationManager
    )
    {
        _serviceProvider = serviceProvider;
        _configurationManager = configurationManager;

        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var factories = ((ConfigurationManager)_configurationManager).ConfigurationUIFactories;

        foreach (
            var factory in factories
                .OrderBy(p => p.Order)
                .ThenBy(p => p.Factory.Title, StringComparer.CurrentCultureIgnoreCase)
        )
        {
            var ui = (UIElement)factory.Factory.CreateControl(_serviceProvider);

            _pages.Items.Add(
                new TreeViewItem
                {
                    Header = factory.Factory.Title,
                    Tag = ui,
                    IsSelected = StartupPage == factory.Factory.Id
                }
            );
        }

        if (_pages.Items.Count > 0 && _pages.SelectedItem == null)
            ((TreeViewItem)_pages.Items[0]!).IsSelected = true;
    }

    private void _pages_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _container.Content = (UIElement?)((TreeViewItem?)e.NewValue)?.Tag;
    }

    private async void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (TreeViewItem page in _pages.Items)
            {
                var task = ((IConfigurationUI)page.Tag).Save();

                // Only disable the window if any of the save operations
                // actually start an asynchronous task.

                if (!task.IsCompleted)
                    IsEnabled = false;

                var status = await task;
                if (status == SaveStatus.Failure)
                    return;
            }

            DialogResult = true;
        }
        finally
        {
            IsEnabled = true;
        }
    }
}
