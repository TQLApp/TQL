namespace Tql.App.Services.Synchronization;

internal interface IBackupProvider
{
    string Label { get; }
    BackupProviderService Service { get; }
    bool IsConfigured { get; }

    void Initialize(SynchronizationService synchronizationService);
    Task Setup(CancellationToken cancellationToken = default);
    Task Remove(CancellationToken cancellationToken = default);
    Task UploadBackup(Backup backup, CancellationToken cancellationToken = default);
    Task<Backup?> DownloadBackup(CancellationToken cancellationToken = default);
    Task<BackupStatus> GetBackupStatus(CancellationToken cancellationToken = default);
}
