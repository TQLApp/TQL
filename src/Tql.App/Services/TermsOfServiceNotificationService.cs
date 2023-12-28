using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.Services;

internal class TermsOfServiceNotificationService(
    IUI ui,
    Settings settings,
    HttpClient httpClient,
    ILogger<TermsOfServiceNotificationService> logger
) : IHostedService
{
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(
            TimerCallback,
            null,
            TimeSpan.Zero,
            Constants.TermsOfServiceCheckInterval
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        _timer = null;

        return Task.CompletedTask;
    }

    private void TimerCallback(object? state)
    {
        try
        {
            TaskUtils.RunSynchronously(CheckTermsOfService);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error while checking for updated terms of service");
        }
    }

    private async Task CheckTermsOfService()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, Constants.TermsOfServiceUrl);

        GitHubUtils.InitializeRequest(request);

        using var response = await httpClient.SendAsync(request);

        await using var stream = await response.Content.ReadAsStreamAsync();

        var bytes = await SHA1.HashDataAsync(stream);

        var hash = Utilities.Encryption.HexEncode(bytes);

        if (settings.LastTermsOfServiceHash != hash)
        {
            // The user has already accepted the terms of service in the MSI installer.
            // We only need to show changes.
            if (settings.LastTermsOfServiceHash != null)
            {
                ui.ShowNotificationBar(
                    GetType().FullName!,
                    Labels.TermsOfServiceNotificationService_Updated,
                    _ => ui.OpenUrl(Constants.TermsOfServiceUrl)
                );
            }

            settings.LastTermsOfServiceHash = hash;
        }
    }
}
