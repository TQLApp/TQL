using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.Services.Packages;

namespace Tql.App.ConfigurationUI;

internal partial class PackageSourcesConfigurationControl : IConfigurationPage
{
    private readonly IEncryption _encryption;
    private readonly PackageManager _packageManager;
    private readonly IServiceProvider _serviceProvider;

    private new PackageManagerConfigurationDto DataContext =>
        (PackageManagerConfigurationDto)base.DataContext;

    public Guid PageId => Constants.PackageManagerPageId;
    public string Title => Labels.PackageSourcesConfiguration_PackageSources;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public PackageSourcesConfigurationControl(
        IEncryption encryption,
        PackageManager packageManager,
        IServiceProvider serviceProvider
    )
    {
        _encryption = encryption;
        _packageManager = packageManager;
        _serviceProvider = serviceProvider;

        InitializeComponent();

        base.DataContext = PackageManagerConfigurationDto.FromConfiguration(
            _packageManager.Configuration,
            _encryption
        );
    }

    public void Initialize(IConfigurationPageContext context) { }

    public Task<SaveStatus> Save()
    {
        _packageManager.UpdateConfiguration(DataContext.ToConfiguration(_encryption));

        return Task.FromResult(SaveStatus.Success);
    }

    private void _add_Click(object? sender, RoutedEventArgs e) => Edit(null);

    private void _edit_Click(object sender, RoutedEventArgs e) =>
        Edit((PackageSourceDto)_sources.SelectedItem);

    private void Edit(PackageSourceDto? connection)
    {
        var editConnection = connection?.Clone() ?? new PackageSourceDto();

        var window = _serviceProvider.GetRequiredService<PackageSourceEditWindow>();
        window.Owner = Window.GetWindow(this);
        window.DataContext = editConnection;

        if (window.ShowDialog().GetValueOrDefault())
        {
            if (connection != null)
                DataContext.Sources[_sources.SelectedIndex] = editConnection;
            else
                DataContext.Sources.Add(editConnection);
        }
    }

    private void _delete_Click(object? sender, RoutedEventArgs e)
    {
        DataContext.Sources.Remove((PackageSourceDto)_sources.SelectedItem);
    }
}
