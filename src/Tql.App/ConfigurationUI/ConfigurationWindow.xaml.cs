using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.QuickStart;
using Tql.App.Services;

namespace Tql.App.ConfigurationUI;

internal partial class ConfigurationWindow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPluginManager _pluginManager;
    private readonly QuickStartScript _quickStartScript;
    private Context? _currentContext;

    public Guid? StartupPage { get; set; }
    public IConfigurationPage? SelectedPage => _currentContext?.Page;

    public event EventHandler? SelectedPageChanged;

    public ConfigurationWindow(
        IServiceProvider serviceProvider,
        IPluginManager pluginManager,
        QuickStartScript quickStartScript
    )
        : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _pluginManager = pluginManager;
        _quickStartScript = quickStartScript;

        InitializeComponent();
    }

    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        var rootNode = AddCategory(
            Labels.ConfigurationCategory_Application,
            GetAppConfigurationPages()
        );

        foreach (
            var plugin in _pluginManager
                .Plugins
                .OrderBy(p => p.Title, StringComparer.CurrentCultureIgnoreCase)
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

        _quickStartScript.HandleConfigurationWindow(this);
    }

    private TreeViewItem AddCategory(string title, IEnumerable<IConfigurationPage> pages)
    {
        var node = new TreeViewItem { Header = title };

        foreach (var page in pages)
        {
            var isSelected = StartupPage == page.PageId;

            var context = new Context(this, page);

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
        yield return _serviceProvider.GetRequiredService<ProfilesConfigurationControl>();
    }

    private void _pages_SelectedItemChanged(
        object? sender,
        RoutedPropertyChangedEventArgs<object> e
    )
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

        SetCurrentContext(context);

        if (context == null && treeViewItem.Items.Count > 0)
        {
            treeViewItem.IsExpanded = true;
            ((TreeViewItem)treeViewItem.Items[0]!).IsSelected = true;
        }
    }

    private void SetCurrentContext(Context? context)
    {
        if (context == null)
        {
            _container.Content = null;
        }
        else
        {
            if (!context.HasBeenVisible)
            {
                context.HasBeenVisible = true;

                context.Page.Initialize(context);
            }

            context.IsVisible = true;

            _container.Content = context.Page;

            _container.VerticalScrollBarVisibility =
                context.Page.PageMode == ConfigurationPageMode.Scroll
                    ? ScrollBarVisibility.Auto
                    : ScrollBarVisibility.Disabled;
        }

        _currentContext = context;

        if (_currentContext != null)
            OnSelectedPageChanged();
    }

    private async void _acceptButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            foreach (var page in GetAllTreeViewItems(_pages.Items))
            {
                if (page.Tag is not Context { HasBeenVisible: true } context)
                    continue;

                // If the page decides to show UI or something, this
                // ensures that it's the current page, and that it can
                // find its owner.

                page.IsSelected = true;

                SetCurrentContext(context);

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

    private void Window_Closed(object? sender, EventArgs e)
    {
        foreach (var treeViewItem in GetAllTreeViewItems(_pages.Items))
        {
            if (treeViewItem.Tag is Context context)
                context.RaiseClosed();
        }
    }

    protected virtual void OnSelectedPageChanged() =>
        SelectedPageChanged?.Invoke(this, EventArgs.Empty);

    private class Context(ConfigurationWindow owner, IConfigurationPage page)
        : IConfigurationPageContext
    {
        private bool _isVisible;

        public IConfigurationPage Page { get; } = page;

        public bool HasBeenVisible { get; set; }

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

        public void RaiseClosed() => OnClosed();

        protected virtual void OnIsVisibleChanged() =>
            IsVisibleChanged?.Invoke(owner, EventArgs.Empty);

        protected virtual void OnClosed() => Closed?.Invoke(owner, EventArgs.Empty);
    }
}
