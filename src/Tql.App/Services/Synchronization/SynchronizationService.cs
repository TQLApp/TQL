using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Tql.Abstractions;
using Tql.App.Services.Database;
using Tql.App.Support;

namespace Tql.App.Services.Synchronization;

internal class SynchronizationService : IDisposable
{
    private readonly LocalSettings _settings;
    private readonly BackupService _backupService;
    private readonly IUI _ui;
    private readonly IConfigurationManager _configurationManager;
    private readonly IDb _db;
    private readonly ILogger<SynchronizationService> _logger;
    private volatile string _synchronizationStatus = default!;
    private readonly Timer _synchronizationTimer;
    private readonly Timer _restoreTimer;
    private Thread? _synchronizeThread;
    private readonly object _syncRoot = new();
    private SynchronizationConfiguration _configuration;
    private readonly ImmutableArray<IBackupProvider> _providers;
    private bool _isConfigured;
    private Backup? _pendingBackup;
    private bool _dirty;

    public string SynchronizationStatus => _synchronizationStatus;

    public bool IsConfigured
    {
        get
        {
            lock (_syncRoot)
            {
                return _isConfigured;
            }
        }
    }

    public SynchronizationConfiguration Configuration
    {
        get
        {
            lock (_syncRoot)
            {
                return _configuration;
            }
        }
        set
        {
            lock (_syncRoot)
            {
                _configuration = value;

                _settings.SynchronizationConfiguration = JsonSerializer.Serialize(value);

                ReloadConfiguration();
            }
        }
    }

    public event EventHandler? SynchronizationStatusChanged;

    public SynchronizationService(
        LocalSettings settings,
        BackupService backupService,
        IEnumerable<IBackupProvider> providers,
        LifecycleService lifecycleService,
        IUI ui,
        IConfigurationManager configurationManager,
        IDb db,
        ILogger<SynchronizationService> logger
    )
    {
        _settings = settings;
        _backupService = backupService;
        _ui = ui;
        _configurationManager = configurationManager;
        _db = db;
        _providers = providers.ToImmutableArray();
        _logger = logger;

        foreach (var provider in _providers)
        {
            provider.Initialize(this);
        }

        _synchronizationTimer = new Timer(
            _ => StartSynchronization(RestartMode.SilentRestart, SynchronizationMode.Default),
            null,
            Timeout.Infinite,
            Timeout.Infinite
        );
        _restoreTimer = new Timer(
            _ => StartSynchronization(RestartMode.SilentRestart, SynchronizationMode.IgnoreDirty),
            null,
            Timeout.Infinite,
            Timeout.Infinite
        );

        var json = _settings.SynchronizationConfiguration;

        _configuration =
            json == null
                ? new SynchronizationConfiguration(null)
                : JsonSerializer.Deserialize<SynchronizationConfiguration>(json)!;

        ReloadConfiguration();

        UpdateSynchronizationStatus();

        lifecycleService.RegisterAfterHostTermination(AfterHostTermination);

        ((Db)db).TransactionCommitted += (_, _) => SetDirty();
        configurationManager.ConfigurationChanged += (_, _) => SetDirty();

        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;

        void SetDirty()
        {
            lock (_syncRoot)
            {
                _dirty = true;
            }
        }
    }

    private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        switch (e.Reason)
        {
            case SessionSwitchReason.ConsoleConnect:
            case SessionSwitchReason.RemoteConnect:
            case SessionSwitchReason.SessionLogon:
            case SessionSwitchReason.SessionUnlock:
                _logger.LogInformation(
                    "Queueing synchronization because of session switch reason '{Reason}'",
                    e.Reason
                );

                QueueRestoreSynchronization();
                break;
        }
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume)
        {
            _logger.LogInformation(
                "Queueing synchronization because of power mode change to '{Mode}'",
                e.Mode
            );

            QueueRestoreSynchronization();
        }
    }

    private void NetworkChange_NetworkAvailabilityChanged(
        object? sender,
        NetworkAvailabilityEventArgs e
    )
    {
        if (e.IsAvailable)
        {
            _logger.LogInformation("Queueing synchronization because the network became available");

            QueueRestoreSynchronization();
        }
    }

    private void AfterHostTermination()
    {
        if (_pendingBackup == null)
            return;

        // This runs after the host terminates. All services should be shutdown
        // now, and we should be able to safely overwrite files like the
        // configuration and database files.

        _logger.LogInformation("Restoring backup");

        try
        {
            _settings.EncryptionKey = Encryption.Protect(_pendingBackup.Metadata.EncryptionKey);

            using (var stream = new MemoryStream(_pendingBackup.Configuration.ToArray()))
            {
                ((ConfigurationManager)_configurationManager).RestoreConfiguration(stream);
            }

            using (var stream = new MemoryStream(_pendingBackup.Database.ToArray()))
            {
                ((Db)_db).Restore(stream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while restoring backup");
        }
    }

    public IBackupProvider GetProvider(BackupProviderService service)
    {
        return _providers.Single(p => p.Service == service);
    }

    private void UpdateSynchronizationStatus()
    {
        if (_synchronizeThread != null)
        {
            _synchronizationStatus = Labels.SynchronizationService_Synchronizing;
        }
        else
        {
            var lastSynchronized = GetLastSynchronized();
            if (lastSynchronized == null)
            {
                _synchronizationStatus = Labels.SynchronizationService_SynchronizationPending;
            }
            else
            {
                _synchronizationStatus = string.Format(
                    Labels.SynchronizationService_LastSynchronized,
                    lastSynchronized.Value.LocalDateTime.ToString(CultureInfo.CurrentCulture)
                );
            }
        }
    }

    private DateTimeOffset? GetLastSynchronized()
    {
        var lastSynchronized = _settings.LastSynchronization;
        if (lastSynchronized == null)
            return null;

        return DateTimeOffset.ParseExact(lastSynchronized, "o", CultureInfo.InvariantCulture);
    }

    private void ReloadConfiguration()
    {
        _logger.LogInformation("Reloading configuration");

        _isConfigured = _providers.Any(p => p.IsConfigured);

        if (_isConfigured)
        {
            var lastSynchronized = GetLastSynchronized();

            var delay = Constants.SynchronizationInterval;

            if (lastSynchronized.HasValue)
            {
                var interval = DateTime.Now - lastSynchronized.Value.LocalDateTime;
                if (interval < delay)
                    delay = interval;
            }

            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            _logger.LogInformation("Resetting synchronization timer to {Delay}", delay);

            _synchronizationTimer.Change(delay, Timeout.InfiniteTimeSpan);
        }
        else
        {
            _logger.LogInformation("Disabling synchronization timer");

            _settings.LastSynchronization = null;

            _synchronizationTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    public void QueueRestoreSynchronization()
    {
        _restoreTimer.Change(Constants.SynchronizationRestoreDelay, Timeout.InfiniteTimeSpan);
    }

    public void StartSynchronization(RestartMode restartMode, SynchronizationMode mode)
    {
        _logger.LogInformation("Triggering synchronization");

        lock (_syncRoot)
        {
            if (_synchronizeThread != null || !_isConfigured)
            {
                _logger.LogWarning(
                    "Not configured or synchronization running, not starting a new synchronization"
                );
                return;
            }

            _synchronizationTimer.Change(Timeout.Infinite, Timeout.Infinite);

            _synchronizeThread = new Thread(() => SynchronizationThread(mode, restartMode));
            _synchronizeThread.Start();

            UpdateSynchronizationStatus();
        }

        OnSynchronizationStatusChanged();
    }

    private void SynchronizationThread(SynchronizationMode mode, RestartMode restartMode)
    {
        try
        {
            try
            {
                TaskUtils.RunSynchronously(() => DoSynchronize(mode, restartMode));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Synchronization failed");
            }
            finally
            {
                lock (_syncRoot)
                {
                    _synchronizeThread = null;

                    UpdateSynchronizationStatus();
                }

                OnSynchronizationStatusChanged();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in synchronization thread");
        }
    }

    private async Task DoSynchronize(SynchronizationMode mode, RestartMode restartMode)
    {
        _synchronizationTimer.Change(Timeout.Infinite, Timeout.Infinite);

        try
        {
            _logger.LogInformation("Synchronizing in mode '{Mode}'", mode);

            var dirty = false;

            lock (_syncRoot)
            {
                if (mode == SynchronizationMode.ForceDirty)
                    _dirty = true;

                if (mode != SynchronizationMode.IgnoreDirty)
                {
                    dirty = _dirty;
                    _dirty = false;
                }
            }

            if (dirty)
            {
                await UploadBackup();
            }
            else
            {
                await RestoreBackupIfAvailable(restartMode);
            }

            _logger.LogInformation("Synchronization complete");

            _settings.LastSynchronization = DateTimeOffset.Now.ToString("o");
        }
        finally
        {
            _synchronizationTimer.Change(
                Constants.SynchronizationInterval,
                Timeout.InfiniteTimeSpan
            );
        }
    }

    private async Task UploadBackup()
    {
        var backup = _backupService.CreateBackup();

        foreach (var provider in _providers.Where(p => p.IsConfigured))
        {
            try
            {
                _logger.LogInformation("Saving backup to service '{Service}'", provider.Service);

                await provider.UploadBackup(backup);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Synchronization failed for provider '{Service}'",
                    provider.Service
                );
            }
        }
    }

    private async Task RestoreBackupIfAvailable(RestartMode restartMode)
    {
        foreach (var provider in _providers.Where(p => p.IsConfigured))
        {
            try
            {
                var status = await provider.GetBackupStatus();

                _logger.LogInformation(
                    "Status from backup service '{Service}' is '{Status}'",
                    provider.Service,
                    status
                );

                if (status == BackupStatus.Available)
                {
                    var restoreStatus = await RestoreBackup(provider.Service, restartMode);
                    if (restoreStatus == BackupRestoreStatus.Restoring)
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Exception while checking and restoring backup from service '{Service}'",
                    provider.Service
                );
            }
        }
    }

    public async Task<BackupRestoreStatus> RestoreBackup(
        BackupProviderService service,
        RestartMode restartMode,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Restoring backup from '{Service}' restart mode '{RestartMode}'",
            service,
            restartMode
        );

        var backup = await GetProvider(service).DownloadBackup(cancellationToken);

        if (backup == null)
        {
            _logger.LogInformation("No backup available");
            return BackupRestoreStatus.Missing;
        }

        if (backup.Metadata.Version > BackupMetadata.CurrentVersion)
        {
            _logger.LogInformation(
                "Backup version '{BackupVersion}' is higher than '{CurrentVersion}'; not restoring backup",
                backup.Metadata.Version,
                BackupMetadata.CurrentVersion
            );
            return BackupRestoreStatus.UnsupportedVersion;
        }

        lock (_syncRoot)
        {
            _pendingBackup = backup;
        }

        _logger.LogInformation("Restarting to restore backup");

        _ui.Shutdown(restartMode);

        return BackupRestoreStatus.Restoring;
    }

    protected virtual void OnSynchronizationStatusChanged() =>
        SynchronizationStatusChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _synchronizeThread?.Join();

        _synchronizationTimer.Dispose();
        _restoreTimer.Dispose();
    }
}

internal record SynchronizationConfiguration(GoogleDriveSynchronizationConfiguration? GoogleDrive);
