using Tql.Abstractions;
using Tql.App.Services.Packages;
using Tql.Utilities;

namespace Tql.App.ConfigurationUI;

internal partial class PackageSourcesConfigurationControl : IConfigurationPage
{
    private readonly IUI _ui;
    private readonly IEncryption _encryption;
    private readonly PackageManager _packageManager;

    private new PackageManagerConfigurationDto DataContext =>
        (PackageManagerConfigurationDto)base.DataContext;

    public Guid PageId => Constants.PackageManagerPageId;
    public string Title => Labels.PackageSourcesConfiguration_PackageSources;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public PackageSourcesConfigurationControl(
        IUI ui,
        IEncryption encryption,
        PackageManager packageManager
    )
    {
        _ui = ui;
        _encryption = encryption;
        _packageManager = packageManager;

        InitializeComponent();

        base.DataContext = PackageManagerConfigurationDto.FromConfiguration(
            _packageManager.Configuration,
            _encryption
        );

        _source.DataContext = null;
    }

    public void Initialize(IConfigurationPageContext context) { }

    public Task<SaveStatus> Save()
    {
        if (_source.DataContext != null)
        {
            _ui.ShowAlert(
                this,
                Labels.PackageSourcesConfiguration_EditingSource,
                Labels.PackageSourcesConfiguration_EditingSourceSubtitle
            );
            return Task.FromResult(SaveStatus.Failure);
        }

        _packageManager.UpdateConfiguration(DataContext.ToConfiguration(_encryption));

        return Task.FromResult(SaveStatus.Success);
    }

    private void _add_Click(object? sender, RoutedEventArgs e)
    {
        _sources.SelectedItem = null;

        _source.DataContext = new PackageManagerSourceDto();
    }

    private void _delete_Click(object? sender, RoutedEventArgs e)
    {
        DataContext.Sources.Remove((PackageManagerSourceDto)_sources.SelectedItem);

        _source.DataContext = null;
    }

    private void _update_Click(object? sender, RoutedEventArgs e)
    {
        var source = (PackageManagerSourceDto)_source.DataContext;

        if (_sources.SelectedItem != null)
            DataContext.Sources[_sources.SelectedIndex] = source;
        else
            DataContext.Sources.Add(source);

        _source.DataContext = null;
        _sources.SelectedItem = null;
    }

    private void _sources_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _source.DataContext = ((PackageManagerSourceDto?)_sources.SelectedItem)?.Clone();
    }

    private void _cancel_Click(object sender, RoutedEventArgs e)
    {
        _source.DataContext = null;
        _sources.SelectedItem = null;
    }
}
