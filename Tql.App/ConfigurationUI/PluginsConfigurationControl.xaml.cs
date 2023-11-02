using Tql.Abstractions;
using Tql.App.Services;
using Tql.App.Services.Packages;

namespace Tql.App.ConfigurationUI;

internal partial class PluginsConfigurationControl : IConfigurationPage
{
    private readonly PackageManager _packageManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUI _ui;

    public Guid PageId => Guid.Parse("96260cfa-5814-4ed4-ac69-fcc63e4f4571");
    public string Title => Labels.PluginsConfiguration_Plugins;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public PluginsConfigurationControl(
        PackageManager packageManager,
        IServiceProvider serviceProvider,
        IUI ui
    )
    {
        _packageManager = packageManager;
        _serviceProvider = serviceProvider;
        _ui = ui;

        InitializeComponent();

        IsEnabled = false;

        Resources.Add("CheckmarkCircle", Images.CheckmarkCircle);

        SetSelectedTab(Tab.Browse);
    }

    public void Initialize(IConfigurationPageContext context)
    {
        context.IsVisibleChanged += (_, _) =>
        {
            if (context.IsVisible)
                _ = ReloadPackages();
        };
    }

    private void SetSelectedTab(Tab tab)
    {
        _browseTab.IsChecked = tab == Tab.Browse;
        _installedTab.IsChecked = tab == Tab.Installed;

        _ = ReloadPackages();
    }

    public Task<SaveStatus> Save()
    {
        return Task.FromResult(SaveStatus.Success);
    }

    private void _browseTab_Checked(object sender, RoutedEventArgs e) => SetSelectedTab(Tab.Browse);

    private void _installedTab_Checked(object sender, RoutedEventArgs e) =>
        SetSelectedTab(Tab.Installed);

    private async Task ReloadPackages()
    {
        IsEnabled = false;

        try
        {
            var packages = (await _packageManager.GetAvailablePackages()).ToList();

            if (_installedTab.IsChecked.GetValueOrDefault())
                packages = packages.Where(p => p.IsInstalled).ToList();

            _browser.ItemsSource = packages;
        }
        finally
        {
            IsEnabled = true;
        }
    }

    private async void _install_Click(object sender, RoutedEventArgs e)
    {
        var package = (Package)((FrameworkElement)sender).DataContext;

        ProgressWindow.Show(
            _serviceProvider,
            this,
            () => _packageManager.InstallPackage(package.Identity.Id)
        );

        ShowRestartButton();

        await ReloadPackages();
    }

    private async void _remove_Click(object sender, RoutedEventArgs e)
    {
        var package = (Package)((FrameworkElement)sender).DataContext;

        _packageManager.RemovePackage(package.Identity.Id);

        ShowRestartButton();

        await ReloadPackages();
    }

    private void ShowRestartButton()
    {
        _restart.Visibility = Visibility.Visible;
        _restartLabel.Visibility = Visibility.Visible;
    }

    private void _restart_Click(object sender, RoutedEventArgs e)
    {
        ((UI)_ui).Shutdown(RestartMode.Restart);
    }

    private enum Tab
    {
        Browse,
        Installed
    }
}
