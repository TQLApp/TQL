using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.Services;

namespace Tql.App.ConfigurationUI;

internal partial class ConfigurationWindow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationManager _configurationManager;
    private readonly IPluginManager _pluginManager;

    public Guid? StartupPage { get; set; }

    public ConfigurationWindow(
        IServiceProvider serviceProvider,
        IConfigurationManager configurationManager,
        IPluginManager pluginManager
    )
    {
        _serviceProvider = serviceProvider;
        _configurationManager = configurationManager;
        _pluginManager = pluginManager;

        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var rootNode = AddCategory("Application", GetAppConfigurationPages());

        foreach (
            var plugin in _pluginManager.Plugins.OrderBy(
                p => p.Title,
                StringComparer.CurrentCultureIgnoreCase
            )
        )
        {
            var pages = plugin.GetConfigurationPages().ToList();
            if (pages.Count > 0)
                AddCategory(plugin.Title, pages);
        }

        if (StartupPage == null)
        {
            rootNode.ExpandSubtree();

            ((TreeViewItem)rootNode.Items[0]).IsSelected = true;
        }
    }

    private TreeViewItem AddCategory(string title, IEnumerable<IConfigurationPage> pages)
    {
        var node = new TreeViewItem { Header = title };

        foreach (var page in pages)
        {
            var isSelected = StartupPage == page.PageId;

            node.Items.Add(
                new TreeViewItem
                {
                    Header = page.Title,
                    Tag = page,
                    IsSelected = isSelected
                }
            );

            if (isSelected)
                node.IsExpanded = true;
        }

        _pages.Items.Add(node);

        return node;
    }

    private IEnumerable<IConfigurationPage> GetAppConfigurationPages()
    {
        yield return _serviceProvider.GetRequiredService<GeneralConfigurationControl>();
        yield return _serviceProvider.GetRequiredService<PluginsConfigurationControl>();
        yield return _serviceProvider.GetRequiredService<PackageSourcesConfigurationControl>();
    }

    private void _pages_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var treeViewItem = (TreeViewItem?)e.NewValue;
        if (treeViewItem == null)
        {
            _container.Content = null;
            return;
        }

        var uiElement = (UIElement?)treeViewItem.Tag;

        if (uiElement == null && treeViewItem.Items.Count > 0)
        {
            treeViewItem.IsExpanded = true;
            ((TreeViewItem)treeViewItem.Items[0]).IsSelected = true;
        }
        else
        {
            _container.Content = uiElement;

            if (uiElement is IConfigurationPage configurationPage)
            {
                _container.VerticalScrollBarVisibility =
                    configurationPage.PageMode == ConfigurationPageMode.Scroll
                        ? ScrollBarVisibility.Auto
                        : ScrollBarVisibility.Disabled;
            }
        }
    }

    private async void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (var page in GetAllTreeViewItems(_pages.Items))
            {
                if (page.Tag is not IConfigurationPage configurationPage)
                    continue;

                var task = configurationPage.Save();

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

    private IEnumerable<TreeViewItem> GetAllTreeViewItems(IEnumerable items)
    {
        foreach (TreeViewItem item in items)
        {
            yield return item;

            foreach (var child in GetAllTreeViewItems(item.Items))
            {
                yield return child;
            }
        }
    }
}
