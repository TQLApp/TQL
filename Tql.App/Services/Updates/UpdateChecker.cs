﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Octokit;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Tql.Abstractions;
using Tql.App.Support;
using Path = System.IO.Path;

namespace Tql.App.Services.Updates;

internal class UpdateChecker : IDisposable
{
    private static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromHours(1);

    private readonly IUI _ui;
    private readonly ILogger<UpdateChecker> _logger;
    private readonly HttpClient _httpClient;
    private readonly Timer _timer;

    public UpdateChecker(IUI ui, ILogger<UpdateChecker> logger, HttpClient httpClient)
    {
        _ui = ui;
        _logger = logger;
        _httpClient = httpClient;
        _timer = new Timer(TimerCallback, null, UpdateCheckInterval, UpdateCheckInterval);
    }

    public bool TryStartUpdate()
    {
        return Task.Run(TryStartUpdateAsync).GetAwaiter().GetResult();
    }

    private async Task<bool> TryStartUpdateAsync()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.github.com/repos/pvginkel/TQL/releases/latest"
        );

        InitializeRequest(request);

        using var response = await _httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();

        var release = JsonSerializer.Deserialize<ReleaseDto>(json)!;
        if (
            !release.TagName.StartsWith("v")
            || !Version.TryParse(release.TagName.Substring(1), out var releaseVersion)
        )
            throw new InvalidOperationException("Unexpected tag format");

        var appVersion = GetAppVersion();

        _logger.LogInformation(
            "Latest release version {ReleaseVersion}, app version {AppVersion}",
            releaseVersion,
            appVersion
        );

        if (appVersion.CompareTo(releaseVersion) >= 0)
            return false;

        var assets = release.Assets
            .Where(p => p.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
            .ToList();

        switch (assets.Count)
        {
            case 0:
                _logger.LogError("Release does not have an MSI file");
                return false;
            case 1:
                break;
            default:
                _logger.LogError("Release has more than one MSI files");
                return false;
        }

        var target = await Download(assets.Single());

        Install(target);

        return true;
    }

    private Version GetAppVersion()
    {
        var appVersion = GetType().Assembly.GetName().Version;
        return appVersion;
    }

    private void InitializeRequest(HttpRequestMessage request)
    {
        request.Headers.UserAgent.Add(
            new ProductInfoHeaderValue("TQL", GetAppVersion().ToString())
        );

#if DEBUG
        // TODO: Remove in the final version. This is only for testing
        // while the repo is private.
        var patToken = Environment.GetEnvironmentVariable("GITHUB_PAT");
        if (patToken?.IsEmpty() == false)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", patToken);
#endif
    }

    private async Task<string> Download(ReleaseAssetDto asset)
    {
        _logger.LogInformation("Downloading setup from {Url}", asset.Url);

        var targetFolder = Path.Combine(
            Path.GetTempPath(),
            $"TqlSetup~{new Random().Next(100000, 999999)}"
        );
        Directory.CreateDirectory(targetFolder);

        var targetFileName = Path.Combine(targetFolder, "Tql.msi");

        using var request = new HttpRequestMessage(HttpMethod.Get, asset.Url);

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

        InitializeRequest(request);

        using var response = await _httpClient.SendAsync(request);
        using var source = await response.Content.ReadAsStreamAsync();
        using var target = File.Create(targetFileName);

        await source.CopyToAsync(target);

        return targetFileName;
    }

    private void Install(string target)
    {
        _logger.LogInformation("Running setup at '{Target}'", target);

        Process.Start(
            new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                WorkingDirectory = Path.GetDirectoryName(target),
                Arguments = $"/i \"{Path.GetFileName(target)}\" /qn",
            }
        );
    }

    private void TimerCallback(object state)
    {
        try
        {
            if (TryStartUpdate())
            {
                _logger.LogInformation("Updating is running; shutting down");

                ((UI)_ui).Shutdown();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking for updates");
        }
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}