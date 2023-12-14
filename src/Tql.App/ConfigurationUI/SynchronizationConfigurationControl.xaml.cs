using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Services.Synchronization;
using Tql.Utilities;

namespace Tql.App.ConfigurationUI;

internal partial class SynchronizationConfigurationControl : IConfigurationPage
{
    private readonly SynchronizationService _synchronizationService;
    private readonly IUI _ui;
    private readonly ILogger<SynchronizationConfigurationControl> _logger;

    public Guid PageId => Constants.SynchronizationPageId;
    public string Title => Labels.SynchronizationConfiguration_Synchronization;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.Scroll;

    public SynchronizationConfigurationControl(
        SynchronizationService synchronizationService,
        IUI ui,
        ILogger<SynchronizationConfigurationControl> logger
    )
    {
        _synchronizationService = synchronizationService;
        _ui = ui;
        _logger = logger;

        InitializeComponent();

        _googleDriveSetupImage.Source = Images.GoogleDrive;
        _googleDriveRemoveImage.Source = Images.GoogleDrive;
    }

    public void Initialize(IConfigurationPageContext context)
    {
        _synchronizationStatus.Content = _synchronizationService.SynchronizationStatus;

        _synchronizationService.SynchronizationStatusChanged +=
            _synchronizationService_SynchronizationStatusChanged;

        context.Closed += (_, _) =>
        {
            _synchronizationService.SynchronizationStatusChanged -=
                _synchronizationService_SynchronizationStatusChanged;
        };

        UpdateEnabled();
    }

    private void _synchronizationService_SynchronizationStatusChanged(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(
            () => _synchronizationStatus.Content = _synchronizationService.SynchronizationStatus
        );
    }

    private void UpdateEnabled()
    {
        var googleDriveProvider = _synchronizationService.GetProvider(
            BackupProviderService.GoogleDrive
        );

        SetVisibility(_setupGoogleDrive, !googleDriveProvider.IsConfigured);
        SetVisibility(_removeGoogleDrive, googleDriveProvider.IsConfigured);

        SetVisibility(_synchronization, _synchronizationService.IsConfigured);
        SetVisibility(_synchronizeNow, _synchronizationService.IsConfigured);
        SetVisibility(_synchronizationStatus, _synchronizationService.IsConfigured);

        void SetVisibility(UIElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public Task<SaveStatus> Save()
    {
        return Task.FromResult(SaveStatus.Success);
    }

    private async void _setupGoogleDrive_Click(object sender, RoutedEventArgs e) =>
        await SetupService(BackupProviderService.GoogleDrive);

    private async void _removeGoogleDrive_Click(object sender, RoutedEventArgs e) =>
        await RemoveService(BackupProviderService.GoogleDrive);

    private void _synchronizeNow_Click(object sender, RoutedEventArgs e)
    {
        _synchronizationService.StartSynchronization();

        _ui.ShowAlert(
            this,
            Labels.SynchronizationConfiguration_SynchronizationStarted,
            icon: DialogIcon.Information
        );
    }

    private async Task SetupService(BackupProviderService service)
    {
        var provider = _synchronizationService.GetProvider(service);

        try
        {
            await provider.Setup();

            _ui.ShowAlert(
                this,
                string.Format(Labels.SynchronizationConfiguration_SetupComplete, provider.Label),
                icon: DialogIcon.Information
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Setup with {Provider} failed", provider.Label);

            _ui.ShowException(this, Labels.SynchronizationConfiguration_SetupFailed, ex);
        }

        UpdateEnabled();
    }

    private async Task RemoveService(BackupProviderService service)
    {
        var provider = _synchronizationService.GetProvider(service);

        var result = _ui.ShowConfirmation(
            this,
            Labels.GeneralConfiguration_AreYouSure,
            string.Format(
                Labels.SynchronizationConfiguration_AreYouSureRemoveService,
                provider.Label
            )
        );

        if (result == DialogResult.Yes)
            await provider.Remove();

        UpdateEnabled();
    }
}
