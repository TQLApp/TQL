using System.Globalization;
using Microsoft.Extensions.Logging;
using Tql.App.Support;

namespace Tql.App.Services.Synchronization;

internal class SynchronizationService : IDisposable
{
    private readonly LocalSettings _settings;
    private readonly BackupService _backupService;
    private readonly ILogger<SynchronizationService> _logger;
    private volatile string _synchronizationStatus = default!;
    private readonly Timer _timer;
    private Thread? _synchronizeThread;
    private readonly object _syncRoot = new();
    private SynchronizationConfiguration _configuration;
    private readonly ImmutableArray<IBackupProvider> _providers;
    private bool _isConfigured;

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
        ILogger<SynchronizationService> logger
    )
    {
        _settings = settings;
        _backupService = backupService;
        _providers = providers.ToImmutableArray();
        _logger = logger;

        foreach (var provider in _providers)
        {
            provider.Initialize(this);
        }

        _timer = new Timer(_ => StartSynchronization(), null, Timeout.Infinite, Timeout.Infinite);

        var json = _settings.SynchronizationConfiguration;

        _configuration =
            json == null
                ? new SynchronizationConfiguration(null)
                : JsonSerializer.Deserialize<SynchronizationConfiguration>(json)!;

        ReloadConfiguration();

        UpdateSynchronizationStatus();
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
        _isConfigured = _providers.Any(p => p.IsConfigured);

        if (!_isConfigured)
        {
            _settings.LastSynchronization = null;

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            return;
        }

        var lastSynchronized = GetLastSynchronized();

        var delay = Constants.SynchronizationInterval;

        if (lastSynchronized.HasValue)
        {
            var interval = DateTimeOffset.Now.LocalDateTime - lastSynchronized.Value.LocalDateTime;
            if (interval < delay)
                delay = interval;
        }

        _timer.Change(delay, Timeout.InfiniteTimeSpan);
    }

    public void StartSynchronization()
    {
        lock (_syncRoot)
        {
            if (_synchronizeThread != null || !_isConfigured)
                return;

            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            _synchronizeThread = new Thread(ThreadProc);
            _synchronizeThread.Start();

            UpdateSynchronizationStatus();
        }

        OnSynchronizationStatusChanged();
    }

    private void ThreadProc()
    {
        try
        {
            try
            {
                TaskUtils.RunSynchronously(() => DoSynchronize(default));
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

    private async Task DoSynchronize(CancellationToken cancellationToken)
    {
        var backup = _backupService.CreateBackup();

        foreach (var provider in _providers)
        {
            try
            {
                if (provider.IsConfigured)
                    await provider.UploadBackup(backup, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Synchronization failed for provider '{Provider}'",
                    provider.GetType()
                );
            }
        }

        _settings.LastSynchronization = DateTimeOffset.Now.ToString("o");
    }

    protected virtual void OnSynchronizationStatusChanged() =>
        SynchronizationStatusChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _synchronizeThread?.Join();

        _timer.Dispose();
    }
}

internal record SynchronizationConfiguration(GoogleDriveSynchronizationConfiguration? GoogleDrive);
