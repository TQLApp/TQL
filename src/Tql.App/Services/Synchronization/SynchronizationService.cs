using System.Globalization;
using Microsoft.Extensions.Logging;
using Tql.App.Support;

namespace Tql.App.Services.Synchronization;

internal partial class SynchronizationService : IDisposable
{
    private readonly LocalSettings _settings;
    private readonly BackupService _backupService;
    private readonly ILogger<SynchronizationService> _logger;
    private volatile string _status;
    private volatile bool _isConfigured;
    private readonly Timer _timer;
    private volatile Thread? _synchronizeThread;

    public string SynchronizationStatus
    {
        get => _status;
        set
        {
            _status = value;

            OnSynchronizationStatusChanged();
        }
    }

    public bool IsSynchronizing => _synchronizeThread != null;
    public bool IsConfigured => _isConfigured;

    public event EventHandler? SynchronizationStatusChanged;

    public SynchronizationService(
        LocalSettings settings,
        BackupService backupService,
        ILogger<SynchronizationService> logger
    )
    {
        _settings = settings;
        _backupService = backupService;
        _logger = logger;

        _timer = new Timer(_ => StartSynchronization(), null, Timeout.Infinite, Timeout.Infinite);

        ReloadConfiguration();

        _status = GetStatus();
    }

    private string GetStatus()
    {
        if (IsSynchronizing)
            return Labels.SynchronizationService_Synchronizing;

        var lastSynchronized = GetLastSynchronized();
        if (lastSynchronized == null)
            return Labels.SynchronizationService_SynchronizationPending;

        return string.Format(
            Labels.SynchronizationService_LastSynchronized,
            lastSynchronized.Value.LocalDateTime.ToString(CultureInfo.CurrentCulture)
        );
    }

    private DateTimeOffset? GetLastSynchronized()
    {
        var lastSynchronized = _settings.LastSynchronization;
        if (lastSynchronized == null)
            return null;

        return DateTimeOffset.ParseExact(lastSynchronized, "o", CultureInfo.InvariantCulture);
    }

    public SynchronizationConfiguration GetConfiguration()
    {
        var json = _settings.SynchronizationConfiguration;
        if (json == null)
            return new SynchronizationConfiguration(null);

        return JsonSerializer.Deserialize<SynchronizationConfiguration>(json)!;
    }

    public void SetConfiguration(SynchronizationConfiguration configuration)
    {
        _settings.SynchronizationConfiguration = JsonSerializer.Serialize(configuration);

        ReloadConfiguration();
    }

    private void ReloadConfiguration()
    {
        var configuration = GetConfiguration();

        _isConfigured = configuration.GoogleDrive != null;

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
        if (!_isConfigured || IsSynchronizing)
            return;

        _timer.Change(Timeout.Infinite, Timeout.Infinite);

        _synchronizeThread = new Thread(ThreadProc);
        _synchronizeThread.Start();

        SynchronizationStatus = GetStatus();
    }

    private void ThreadProc()
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
            _synchronizeThread = null;
            SynchronizationStatus = GetStatus();
        }
    }

    private async Task DoSynchronize(CancellationToken cancellationToken)
    {
        var backup = _backupService.CreateBackup();

        var configuration = GetConfiguration();

        if (configuration.GoogleDrive != null)
            await DoGoogleDriveSynchronization(backup, cancellationToken);

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

internal record GoogleDriveSynchronizationConfiguration;
