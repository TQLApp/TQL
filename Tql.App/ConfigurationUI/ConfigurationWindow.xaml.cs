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
    private Context? _currentContext;

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

            var context = new Context(page);

            page.Initialize(context);

            node.Items.Add(
                new TreeViewItem
                {
                    Header = page.Title,
                    Tag = context,
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
        if (_currentContext != null)
            _currentContext.IsVisible = false;

        var treeViewItem = (TreeViewItem?)e.NewValue;
        if (treeViewItem == null)
        {
            _currentContext = null;
            _container.Content = null;
            return;
        }

        var context = (Context)treeViewItem.Tag;

        if (context == null)
        {
            _container.Content = null;

            if (treeViewItem.Items.Count > 0)
            {
                treeViewItem.IsExpanded = true;
                ((TreeViewItem)treeViewItem.Items[0]).IsSelected = true;
            }
        }
        else
        {
            context.IsVisible = true;

            _container.Content = context.Page;

            _container.VerticalScrollBarVisibility =
                context.Page.PageMode == ConfigurationPageMode.Scroll
                    ? ScrollBarVisibility.Auto
                    : ScrollBarVisibility.Disabled;
        }

        _currentContext = context;
    }

    private async void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            foreach (var page in GetAllTreeViewItems(_pages.Items))
            {
                if (page.Tag is not Context context)
                    continue;

                var task = context.Page.Save();

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

    private void Window_Closed(object sender, EventArgs e)
    {
        foreach (var treeViewItem in GetAllTreeViewItems(_pages.Items))
        {
            if (treeViewItem.Tag is Context context)
                context.RaiseClosed();
        }
    }

    private class Context : IConfigurationPageContext
    {
        private bool _isVisible;

        public IConfigurationPage Page { get; }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnIsVisibleChanged();
                }
            }
        }

        public event EventHandler? IsVisibleChanged;
        public event EventHandler? Closed;

        public Context(IConfigurationPage page)
        {
            Page = page;
        }

        public void RaiseClosed() => OnClosed();

        protected virtual void OnIsVisibleChanged() =>
            IsVisibleChanged?.Invoke(this, EventArgs.Empty);

        protected virtual void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);
    }
}
