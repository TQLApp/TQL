using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;

namespace Tql.App.Services.Synchronization;

internal partial class SynchronizationService
{
    private const string ClientId =
        "596960957664-d847qfgjba8qr1bjq3j801d0tk3hn7ia.apps.googleusercontent.com";
    private const string ClientSecret = "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";

    public async Task SetupGoogleDrive(CancellationToken cancellationToken = default)
    {
        await GetGoogleDriveCredentials(cancellationToken);

        SetConfiguration(
            GetConfiguration() with
            {
                GoogleDrive = new GoogleDriveSynchronizationConfiguration()
            }
        );
    }

    private Task<UserCredential> GetGoogleDriveCredentials(CancellationToken cancellationToken)
    {
        var clientSecrets = new ClientSecrets { ClientId = ClientId, ClientSecret = ClientSecret };
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

    public async Task RemoveGoogleDrive(CancellationToken cancellationToken = default)
    {
        try
        {
            var credentials = await GetGoogleDriveCredentials(cancellationToken);

            await credentials.RevokeTokenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to revoke Google Drive token");
        }

        SetConfiguration(GetConfiguration() with { GoogleDrive = null });
    }

    private async Task DoGoogleDriveSynchronization(
        Backup backup,
        CancellationToken cancellationToken
    )
    {
        var service = new DriveService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = await GetGoogleDriveCredentials(cancellationToken),
                ApplicationName = Labels.ApplicationTitle
            }
        );

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = "backup.zip",
            Parents = new List<string> { "appDataFolder" }
        };

        using var stream = new MemoryStream();

        backup.Serialize(stream);

        stream.Position = 0;

        var request = service.Files.Create(fileMetadata, stream, "application/zip");

        var progress = await request.UploadAsync(cancellationToken);

        if (progress.Exception != null)
            throw progress.Exception;
    }
}
