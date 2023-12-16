using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using File = Google.Apis.Drive.v3.Data.File;

namespace Tql.App.Services.Synchronization;

internal class GoogleDriveBackupProvider(ILogger<GoogleDriveBackupProvider> logger)
    : IBackupProvider
{
    private const string ClientId =
        "596960957664-d847qfgjba8qr1bjq3j801d0tk3hn7ia.apps.googleusercontent.com";
    private const string BackupFolder = "appDataFolder";
    private const string BackupFileName = "backup.zip";

    private SynchronizationService _synchronizationService = default!;

    public string Label => Labels.GoogleDriveBackupProvider_Label;
    public BackupProviderService Service => BackupProviderService.GoogleDrive;
    public bool IsConfigured => _synchronizationService.Configuration.GoogleDrive != null;

    public void Initialize(SynchronizationService synchronizationService)
    {
        _synchronizationService = synchronizationService;
    }

    public async Task Setup(CancellationToken cancellationToken)
    {
        await GetCredentials(cancellationToken);

        _synchronizationService.Configuration = _synchronizationService.Configuration with
        {
            GoogleDrive = new GoogleDriveSynchronizationConfiguration(null)
        };
    }

    private Task<UserCredential> GetCredentials(CancellationToken cancellationToken)
    {
        var clientSecrets = new ClientSecrets
        {
            ClientId = ClientId,
            ClientSecret = GetClientSecret()
        };
        string userId = "DefaultUser";
        if (App.Options.Environment != null)
            userId = $"{userId}.{App.Options.Environment}";

        return GoogleWebAuthorizationBroker.AuthorizeAsync(
            clientSecrets,
            new[] { DriveService.Scope.DriveAppdata },
            userId,
            cancellationToken
        );
    }

    private string GetClientSecret()
    {
#if DEBUG
        return Environment.GetEnvironmentVariable("GOOGLE_DRIVE_API_SECRET")!;
#else
        return Encoding.UTF8.GetString(
            Convert.FromBase64String("""<![SECRET[GOOGLE_DRIVE_API_SECRET]]>""")
        );
#endif
    }

    public async Task Remove(CancellationToken cancellationToken)
    {
        try
        {
            var credentials = await GetCredentials(cancellationToken);

            await credentials.RevokeTokenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to revoke Google Drive token");
        }

        _synchronizationService.Configuration = _synchronizationService.Configuration with
        {
            GoogleDrive = null
        };
    }

    public async Task<BackupStatus> GetBackupStatus(CancellationToken cancellationToken)
    {
        using var service = await GetDriveService(cancellationToken);

        var file = await GetBackupFile(service, cancellationToken);

        if (file == null)
            return BackupStatus.Missing;

        var configuration = _synchronizationService.Configuration.GoogleDrive!;

        if (
            string.Equals(
                configuration.BackupVersion,
                file.Md5Checksum,
                StringComparison.OrdinalIgnoreCase
            )
        )
            return BackupStatus.UpToDate;

        return BackupStatus.Available;
    }

    private static async Task<File?> GetBackupFile(
        DriveService service,
        CancellationToken cancellationToken
    )
    {
        var list = service.Files.List();

        list.Spaces = BackupFolder;
        list.Fields = "nextPageToken, files(id, name, md5Checksum)";

        var files = await list.ExecuteAsync(cancellationToken);

        var backupFiles = files
            .Files.Where(
                p => string.Equals(p.Name, BackupFileName, StringComparison.OrdinalIgnoreCase)
            )
            .ToList();

        // Automatically reset any invalid state.

        if (backupFiles.Count > 1)
        {
            foreach (var file in backupFiles)
            {
                await service.Files.Delete(file.Id).ExecuteAsync(cancellationToken);
            }
        }

        if (backupFiles.Count == 1)
            return backupFiles.Single();

        return null;
    }

    public async Task UploadBackup(Backup backup, CancellationToken cancellationToken)
    {
        using var service = await GetDriveService(cancellationToken);

        using var stream = new MemoryStream();

        backup.Serialize(stream);

        stream.Position = 0;

        var file = await GetBackupFile(service, cancellationToken);
        string md5Checksum;

        if (file == null)
        {
            var metadata = new File { Name = BackupFileName, Parents = new[] { BackupFolder } };
            var request = service.Files.Create(metadata, stream, "application/zip");

            request.Fields = "md5Checksum";

            var progress = await request.UploadAsync(cancellationToken);
            if (progress.Exception != null)
                throw progress.Exception;

            md5Checksum = request.ResponseBody.Md5Checksum;
        }
        else
        {
            var metadata = new File { Name = BackupFileName };
            var request = service.Files.Update(metadata, file.Id, stream, "application/zip");

            request.Fields = "md5Checksum";

            var progress = await request.UploadAsync(cancellationToken);
            if (progress.Exception != null)
                throw progress.Exception;

            md5Checksum = request.ResponseBody.Md5Checksum;
        }

        _synchronizationService.Configuration = _synchronizationService.Configuration with
        {
            GoogleDrive = new GoogleDriveSynchronizationConfiguration(md5Checksum)
        };
    }

    public async Task<Backup?> DownloadBackup(CancellationToken cancellationToken = default)
    {
        using var service = await GetDriveService(cancellationToken);

        var file = await GetBackupFile(service, cancellationToken);

        if (file == null)
            return null;

        _synchronizationService.Configuration = _synchronizationService.Configuration with
        {
            GoogleDrive = new GoogleDriveSynchronizationConfiguration(file.Md5Checksum)
        };

        var request = service.Files.Get(file.Id);

        request.Alt = DriveBaseServiceRequest<File>.AltEnum.Media;

        await using var stream = await request.ExecuteAsStreamAsync(cancellationToken);

        return Backup.Deserialize(stream);
    }

    private async Task<DriveService> GetDriveService(CancellationToken cancellationToken)
    {
        return new DriveService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = await GetCredentials(cancellationToken),
                ApplicationName = Labels.ApplicationTitle
            }
        );
    }
}

internal record GoogleDriveSynchronizationConfiguration(string? BackupVersion);
